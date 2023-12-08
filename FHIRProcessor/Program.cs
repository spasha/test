using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Timers;
using log4net;
using log4net.Config;
using FHIR.Business;
using FHIR.Business.Services;
using VA.FSC.Services.Common.Enums;

namespace VA.FSC.Services
{
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static FHIRProcessor _fhirProcessor;
        private static readonly DatabaseMaintenanceService _databaseMaintenanceService;
        private static readonly RmqUptimeAnalysisService _rmqUptimeAnalysisService;
        private static readonly F277PerformanceAnalysisService _f277PerformanceAnalysisService;
        private static readonly string _fileOutputJsonAsynchronousMetadataUpdatesEnabled = ConfigurationManager.AppSettings["FileOutputJsonAsynchronousMetadataUpdatesEnabled"];
        private static readonly DailyReportService _dailyReportService; 

        static Program()
        {
            _databaseMaintenanceService = new DatabaseMaintenanceService();
            _rmqUptimeAnalysisService = new RmqUptimeAnalysisService();
            _f277PerformanceAnalysisService = new F277PerformanceAnalysisService();
            _dailyReportService = new DailyReportService();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            XmlConfigurator.Configure(); //log4net
            MDC.Set("machine", Environment.MachineName); // log4net

            // Set Up Timer for Chron Jobs
            var chronJobTimerIntervalMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["ChronJobTimerIntervalMilliseconds"]);
            
            var chronJobTimer = new System.Timers.Timer(chronJobTimerIntervalMilliseconds); 
            chronJobTimer.Elapsed += new ElapsedEventHandler(OnChronJobTimedEvent);
            chronJobTimer.Start();

            Log.Debug("Starting FHIR Processor Main");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
           
            try
            {
                StartService();
            }
            catch (Exception ex)
            {
                Log.Error("Fatal Exception on Initial Service Startup - Cannot Execute Service!", ex); // ErrorLevel.Fatal
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled Exception from " + sender.ToString(), e.ExceptionObject as Exception); // ErrorLevel.Critical
        }

        private static void StartService()
        {
            Log.Info("Starting FHIR Processor");

            if (Environment.UserInteractive)
            {
                using (var service = new FHIRProcessor())
                {
                    service.Initialize();
                }
            }
            else
            {
                _fhirProcessor = new FHIRProcessor();
                ServiceBase[] ServicesToRun = new ServiceBase[] { _fhirProcessor };
                ServiceBase.Run(ServicesToRun);
            }
        }
        private static void OnChronJobTimedEvent(object source, ElapsedEventArgs e)
        {
            RunChronJobs();
        }

        public static void RunChronJobs()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                TruncateDatabaseLogs();
            });

            System.Threading.Tasks.Task.Run(() =>
            {
                AnalyzeRmqUptimePerformance();
            });

            System.Threading.Tasks.Task.Run(() =>
            {
                AnalyzeF277Performance();
            });
            System.Threading.Tasks.Task.Run(() =>
            {
                if (ConfigurationManager.AppSettings["DailyReportIsEnabled"] == "true")
                {
                    CreateAndDistributeDailyReport();
                }
            });

        }


        public static void CreateAndDistributeDailyReport()
        {
            int dailyReportHourOfDayToCreateReport = Convert.ToInt32(ConfigurationManager.AppSettings["DailyReportHourOfDayToCreateReport"]);
            int dailyReportHourOfDayToDistributeReport = Convert.ToInt32(ConfigurationManager.AppSettings["DailyReportHourOfDayToDistributeReport"]);
                      
            if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            {
                _dailyReportService.GenerateTransactionDataAndCreateReportFile(DateTime.Now,
                                                                               false,
                                                                               dailyReportHourOfDayToCreateReport);
            }
        }



        public static void TruncateDatabaseLogs()
        {
            int truncateDatabaseLogAfterNumberOfDays = Convert.ToInt32(ConfigurationManager.AppSettings["TruncateDatabaseLogAfterNumberOfDays"]);
            int hourOfDayToRunDatabaseLogTruncation = Convert.ToInt32(ConfigurationManager.AppSettings["HourOfDayToRunDatabaseLogTruncation"]); 
            int maxDatabaseLogRecordsToDeletePerPoll = Convert.ToInt32(ConfigurationManager.AppSettings["MaxDatabaseLogRecordsToDeletePerPoll"]);
            _databaseMaintenanceService.TruncateDatabaseLog(hourOfDayToRunDatabaseLogTruncation, 
                                                            truncateDatabaseLogAfterNumberOfDays,
                                                            maxDatabaseLogRecordsToDeletePerPoll);
        }

        public static void AnalyzeRmqUptimePerformance()
        {
            int hourOfDayToRunRmqUptimeAnalysis = Convert.ToInt32(ConfigurationManager.AppSettings["HourOfDayToRunRmqUptimeAnalysis"]);

            var rmqUptimeAnalysisStartSecond1 = Convert.ToInt32(ConfigurationManager.AppSettings["RmqUptimeAnalysisStartSecond1"]);
            var rmqUptimeAnalysisEndSecond1 = Convert.ToInt32(ConfigurationManager.AppSettings["RmqUptimeAnalysisEndSecond1"]);
            var rmqUptimeAnalysisStartSecond2 = Convert.ToInt32(ConfigurationManager.AppSettings["RmqUptimeAnalysisStartSecond2"]);
            var rmqUptimeAnalysisEndSecond2 = Convert.ToInt32(ConfigurationManager.AppSettings["RmqUptimeAnalysisEndSecond2"]);

            var rmqUptimeAnalysisStartDate1 = DateTime.Now.Date.AddSeconds(rmqUptimeAnalysisStartSecond1);
            var rmqUptimeAnalysisEndDate1 = DateTime.Now.Date.AddSeconds(rmqUptimeAnalysisEndSecond1);
            var rmqUptimeAnalysisStartDate2 = DateTime.Now.Date.AddSeconds(rmqUptimeAnalysisStartSecond2);
            var rmqUptimeAnalysisEndDate2 = DateTime.Now.Date.AddSeconds(rmqUptimeAnalysisEndSecond2);
            
            // Compute Listener, Time Window 1
            _rmqUptimeAnalysisService.CalculateAndStoreRmqUptimeMetrics(hourOfDayToRunRmqUptimeAnalysis,
                                                                          rmqUptimeAnalysisStartDate1,
                                                                          rmqUptimeAnalysisEndDate1,
                                                                          ApplicationNames.FHIRListener.ToString());
            // Compute Listener, Time Window 2
            _rmqUptimeAnalysisService.CalculateAndStoreRmqUptimeMetrics(hourOfDayToRunRmqUptimeAnalysis,
                                                                          rmqUptimeAnalysisStartDate2,
                                                                          rmqUptimeAnalysisEndDate2,
                                                                          ApplicationNames.FHIRListener.ToString());
            // Compute Processor, Time Window 1
            _rmqUptimeAnalysisService.CalculateAndStoreRmqUptimeMetrics(hourOfDayToRunRmqUptimeAnalysis,
                                                                          rmqUptimeAnalysisStartDate1,
                                                                          rmqUptimeAnalysisEndDate1,
                                                                          ApplicationNames.FHIRProcessor.ToString());
            // Compute Processor, Time Window 2
            _rmqUptimeAnalysisService.CalculateAndStoreRmqUptimeMetrics(hourOfDayToRunRmqUptimeAnalysis,
                                                                          rmqUptimeAnalysisStartDate2,
                                                                          rmqUptimeAnalysisEndDate2,
                                                                          ApplicationNames.FHIRProcessor.ToString());
        }

        public static void AnalyzeF277Performance()
        {
            int hourOfDayToRunF277PerformanceAnalysis = Convert.ToInt32(ConfigurationManager.AppSettings["HourOfDayToRunF277PerformanceAnalysis"]);
            var f277PerformanceAnalysisStudyStartSecond = Convert.ToInt32(ConfigurationManager.AppSettings["F277PerformanceAnalysisStudyStartSecond"]);
            var f277PerformanceAnalysisStudyEndSecond = Convert.ToInt32(ConfigurationManager.AppSettings["F277PerformanceAnalysisStudyEndSecond"]);

            var f277PerformanceAnalysisStudyStartDate = DateTime.Now.Date.AddSeconds(f277PerformanceAnalysisStudyStartSecond);
            var f277PerformanceAnalysisStudyEndDate = DateTime.Now.Date.AddSeconds(f277PerformanceAnalysisStudyEndSecond);


            _f277PerformanceAnalysisService.CalculateAndStoreF277PerformanceMetrics(hourOfDayToRunF277PerformanceAnalysis,
                                                                                    f277PerformanceAnalysisStudyStartDate,
                                                                                    f277PerformanceAnalysisStudyEndDate);
        }
    }


    class MyService : FHIRProcessor.IService
    {
        public void Start() { }
        public void Dispose() { }
    }

    [RunInstaller(true)]
    public class FSCServiceInstaller : Installer
    {
        private const string DISPLAY_NAME = "VA FHIR Processing Service";
        private const string DESCRIPTION = "A service allowing encapsulated processes to execute continuously.";
        private const ServiceAccount ACCOUNT_TYPE = ServiceAccount.User;
        private const ServiceStartMode START_MODE = ServiceStartMode.Automatic;
        private const bool DELAYED_START = true;

        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;

        public FSCServiceInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();
            processInstaller.Account = ACCOUNT_TYPE;
            serviceInstaller.ServiceName = ApplicationNames.FHIRProcessor.ToString();
            serviceInstaller.DisplayName = DISPLAY_NAME;
            serviceInstaller.Description = DESCRIPTION;
            serviceInstaller.StartType = START_MODE;
            serviceInstaller.DelayedAutoStart = DELAYED_START;
            this.AfterInstall += new InstallEventHandler(OnAfterInstall);
            this.BeforeUninstall += new InstallEventHandler(OnBeforeUninstall);
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }

        public string GetContextParameter(string key)
        {
            if (this.Context.Parameters.ContainsKey(key))
            {
                return this.Context.Parameters[key].ToString();
            }
            else
            {
                return "";
            }
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            if (processInstaller.Account != ServiceAccount.User) return;

            string usr = GetContextParameter("user").Trim();
            string pwd = GetContextParameter("password").Trim();

            if (!string.IsNullOrEmpty(usr))
                processInstaller.Username = usr;
            if (!string.IsNullOrEmpty(pwd))
                processInstaller.Password = pwd;
        }

        public void OnAfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName))
            {
                sc.Start();
            }
        }

        public void OnBeforeUninstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName))
            {
                if (sc.Status != ServiceControllerStatus.Stopped)
                {
                    sc.Stop();
                }
            }
        }
    }
}
