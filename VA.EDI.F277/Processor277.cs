using System;
using System.Reflection;
using VA.FSC.Services;
using Timer = System.Timers.Timer;
using System.Text;
using FHIR.DAL;
using FHIR.DAL.F277;
using log4net;
using log4net.Config;
using VA.FSC.Services.Common.Enums;
using VA.FSC.Services.Common.Helpers;
using System.Configuration;
using System.Threading;
using FHIR.Business.Services;
using System.IO;
using VA.FSC.Services.Common.DTO;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using FSC.ServiceBus;
using FSC.ServiceBus.Contracts;
using FSC.ServiceBus.Receiver;
using Microsoft.Extensions.Hosting;
using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;
using Microsoft.Extensions.DependencyInjection;
using VA.EDI.F277.ServiceBus;

namespace VA.EDI.F277
{
    public class Processor277 : ProcessorBase, IProcessor
    {
        ////private readonly IMessageMetadataAccessor _metaData;

        private static readonly ILog Log = LogManager.GetLogger(typeof(Processor277));
       
        private string _queue;
        private Timer _enqueuementTimer;
        private Timer _inactivityTimer;
        private DateTime _lastSent;
        private const int _maxHeaderUpdateAttemptCount = 5;

        public string Application { get { return "F277"; } }
        private FhirDal _fhirDal;
        public event EventHandler<ProcessorEventArgs> ProcessingEvent;
        private readonly F277PerformanceAnalysisService _f277PerformanceAnalysisService;
        private readonly EmailService _emailService;
              
        public bool isCurrentlyProcessing = false;
        private readonly string _applicationName = ApplicationNames.FHIRProcessor.ToString();
        private readonly int _f277ProcessorInactivityAlertTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["F277ProcessorInactivityAlertTimeMinutes"]);
        private readonly int _sqlReconnectTimePeriodMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["SqlReconnectTimePeriodMilliseconds"]);
        private readonly string _rmqEnqueuementEnabled = ConfigurationManager.AppSettings["RmqEnqueuementEnabled"];
        private readonly string _fileOutputJsonEnabled = ConfigurationManager.AppSettings["FileOutputJsonEnabled"];
        private readonly string _fileOutputJsonRootPath = ConfigurationManager.AppSettings["FileOutputJsonRootPath"];
        private readonly string _fileOutputJsonSynchronousMetadataUpdatesEnabled = ConfigurationManager.AppSettings["FileOutputJsonSynchronousMetadataUpdatesEnabled"];
        private readonly string _fileOutputJsonAsynchronousMetadataUpdatesEnabled = ConfigurationManager.AppSettings["FileOutputJsonAsynchronousMetadataUpdatesEnabled"];
        IHostBuilder builder;
        IHost _host;
        IDispatchSender service;

        public Processor277(IHost passedHost, string processingConnectionString = "")
                            : base(processingConnectionString)
        {
            var newProcessingConnectionString = processingConnectionString;
            _f277PerformanceAnalysisService = new F277PerformanceAnalysisService();
            _emailService = new EmailService();
            _host = passedHost;
            service = _host.Services.GetService<IDispatchSender>();
            Initialize();
            _fhirDal = new FhirDal();
            _lastSent = DateTime.Now;
            _queue = ConfigurationManager.AppSettings["RmqQueue"];
        }

        public void Initialize()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            XmlConfigurator.Configure(); //log4net
            MDC.Set("machine", Environment.MachineName); // log4net
           
            Log.Debug("Initializing Processor 277. Version: " + version);

            try
            {

                if (Environment.UserInteractive)
                {
                    _fhirDal = new FhirDal();
                    //Data Source = vafscMul5204,44444; Initial Catalog = ECD_FHIR; Integrated Security = True; Encrypt = yes; trustServerCertificate = True;

                    var sentSuccessfully = Process277s();
                }
                else
                {
                    _enqueuementTimer = new Timer(Convert.ToInt32(ConfigurationManager.AppSettings["TimeIntervalForEnqueuing277Milliseconds"]));
                    _inactivityTimer = new Timer(Convert.ToInt32(ConfigurationManager.AppSettings["F277ProcessorInactivityAlertTimeMinutes"]));
                    _enqueuementTimer.Elapsed += Enqueuement_Timer_Elapsed;
                    _inactivityTimer.Elapsed += Inactivity_Timer_Elapsed;
                    _inactivityTimer.Start();
                }



                //////var rabbitMqConfig = new RabbitMQConfig(ConfigurationManager.AppSettings["RmqServer"],
                //////    Convert.ToInt32(ConfigurationManager.AppSettings["RmqPort"]),
                //////    ConfigurationManager.AppSettings["RmqVirtualHost"],
                //////    ConfigurationManager.AppSettings["RmqCertificatePath"],
                //////    ConfigurationManager.AppSettings["RmqUser"],
                //////    ConfigurationManager.AppSettings["RmqPwd"]);
             
                //////_rmqWrapper = new RMQWrapper(rabbitMqConfig);
                //////_rmqWrapper.ErrorEvent += Rmq_ErrorEvent;
                //////_rmqWrapper.Heard += Rmq_Heard;
                
                Log.Info("RMQ Connection Configured");
            }
            catch (Exception ex)
            {
                Log.Error("Error Initializing Processor 277", ex);
            }
        }
       
        private void Enqueuement_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _enqueuementTimer.Stop();
            var sentSuccessfully = Process277s();
            if(sentSuccessfully)
                _enqueuementTimer.Start();
        }
        private void Inactivity_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now - _lastSent > TimeSpan.FromMinutes(_f277ProcessorInactivityAlertTimeMinutes))
            {
                _lastSent = DateTime.Now;
                Log.Info("Have not picked up a 277 status to send to queue " + _queue + " in " + _f277ProcessorInactivityAlertTimeMinutes + " or more minutes");
            }
        }

        ////////private void Rmq_Heard(HeardEventArgs args)
        ////////{
        ////////    // we're not listening 
        ////////    Log.Info($"Unexpected Message Received on RMQ Port {_rmqWrapper.Configuration.Port}!: {args.ToString().AntiLogForging()}");
        ////////}

        ////////private void Rmq_ErrorEvent(string message, Exception ex, System.Diagnostics.Tracing.EventLevel level)
        ////////{
        ////////    Log.Error(message.AntiLogForging(), ex);
        ////////}

        public void RunProcess()
        {
            XmlConfigurator.Configure(); //log4net
            MDC.Set("machine", Environment.MachineName); // log4net
            var maxInitialRmqConnectionAttemptCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxInitialRmqConnectionAttemptCount"]);
            var timeIntervalBetweenInitialRmqConnectionAttemptsMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["TimeIntervalBetweenInitialRmqConnectionAttemptsMilliseconds"]);

            Log.Debug("Running Processor: F277");
            try
            {

                var hasStarted = false;
                // Try to get RMQ connection
                for (int loopCount = 0; loopCount <= maxInitialRmqConnectionAttemptCount; loopCount++)
                {
                    if (!hasStarted)
                    {
                        //////if (_rmqWrapper.Start(_applicationName) == true)
                        //////{
                            _enqueuementTimer.Start();
                            Log.Info("F277 Process Has Started");
                            hasStarted = true;
                            break;
                        //////}
                        //////else
                        //////{
                        //////    Log.Error("Not Able to Get Initial RMQ Connection.  Retrying After " + timeIntervalBetweenInitialRmqConnectionAttemptsMilliseconds + " Milliseconds");
                        //////    Thread.Sleep(timeIntervalBetweenInitialRmqConnectionAttemptsMilliseconds);
                        //////    hasStarted = false;
                        //////}
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error Starting Process F277", ex); // ErrorLevel.Critical
                ProcessingEvent(this, new ProcessorEventArgs("Critical failure initializing process",ErrorLevel.Fatal, "", ex));
            }
        }

        public bool Process277s()
        {
            if (isCurrentlyProcessing == false)
            {
                Log.Info("Polling Database For New Batch of Claim Acknowledgments To Process");
                try
                {
                    isCurrentlyProcessing = true;
                    int lastClaimAcknowledmentHeaderId = 0;
                    int attemptCount = 0;

                    while (isCurrentlyProcessing)
                    {
                        List<DataDto> claims = _fhirDal.GetClaimsBatch(int.Parse(ConfigurationManager.AppSettings["BatchSize"]));
                        if(claims == null || claims.Count < 1) // Claim will be null in the event of db error
                        {
                            isCurrentlyProcessing = false;
                            _inactivityTimer.Start();
                            if (claims == null) // if there is db error, exit method and try again next time
                                return true;
                        }
                        else
                        {
                            if (claims[0].Fhir277DataDto != null)
                            {
                                if (lastClaimAcknowledmentHeaderId == claims[0].Fhir277DataDto.ClaimAcknowledgmentHeaderId) // Indicates a problem
                                {
                                    attemptCount++;

                                    if (attemptCount > _maxHeaderUpdateAttemptCount)
                                    {
                                        Log.Error("Cannot set the flag on the dbo.TempJSONExtract table for header ID: " + claims[0].Fhir277DataDto.ClaimAcknowledgmentHeaderId);
                                        Log.Error(
                                                    "The 277 process in application: " + ApplicationNames.FHIRProcessor.ToString() +
                                                    "  at: " + DateTime.Now +

                                                    " was not able to update the JsonExtractCreated column of the dbo.TempJSONExtract table after 5 tries over" +

                                                    " a total time span of " + _sqlReconnectTimePeriodMilliseconds * 5 + " milliseconds.  In order to prevent endless" +
                                                    " duplicated enqueuements, the 277 processor has been disabled.  Please have a Systems Administrator restart the" +
                                                    " FHIRProcessor service and ensure that there is database connectivity, and that the systems account that the FHIRProcessor" +
                                                    " is running under has permissions to update the StatusFlag column of the 277.ClaimAcknowledgement table."
                                                 );
                                        _emailService.Send277DatabaseUpdateFailureEmails();
                                        return false;
                                    }

                                    Thread.Sleep(_sqlReconnectTimePeriodMilliseconds); // Let SQL try to reconnect first
                                }
                            }

                            if (claims[0].Fhir277DataDto != null)
                                lastClaimAcknowledmentHeaderId = claims[0].Fhir277DataDto.ClaimAcknowledgmentHeaderId;
                            foreach (DataDto claim in claims)
                            {

                                try
                                {
                                    if (claim.QueryIsValid == false)
                                    {
                                        Log.Info("Setting StatusFlag for ClaimAcknowledgmentHeaderId: " + claim.Fhir277DataDto.ClaimAcknowledgmentHeaderId + " To 99 (Error)");
                                        _emailService.SendDataIntegrityEmails(claim.Fhir277DataDto.ClaimAcknowledgmentHeaderId);
                                    }
                                    else
                                    {
                                        _inactivityTimer.Stop();
                                        _lastSent = DateTime.Now;
                                        ProcessClaimAcknowledgmentHeaderRow(claim, false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (claim != null)
                                    {
                                        _emailService.SendDataIntegrityEmails(claim.Fhir277DataDto.ClaimAcknowledgmentHeaderId);
                                    }
                                    Log.Error("Error Processing 277s", ex);
                                }
                            }
                            _fhirDal.UpdateClaimBatchStatus(claims);
                        }
                    }
                    _fhirDal.CleanupAndUpdate();
                }
                catch (Exception ex)
                {
                    isCurrentlyProcessing = false;
                    Log.Error("Error Processing 277s", ex);
                    return false;
                }
            }
            
            return true;
        }

        public void ProcessClaimAcknowledgmentHeaderRow(DataDto dataDto, bool isUnitTest)
        {
            int claimAcknowledgementHeaderId = dataDto.Fhir277DataDto.ClaimAcknowledgmentHeaderId;
            string json = dataDto.ToJson(isUnitTest);


            //Log.Info("FHIR Claim Acknowledgement Message Built, Header ID: " + claimAcknowledgementHeaderId);

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    // Enqueuement
                    if (_rmqEnqueuementEnabled == "true")
                    {
                        service.SendDispatches(new[] { json });

                        //////EnqueueJson(claimAcknowledgementHeaderId, json);
                    }
                    //else
                    //    Log.Info("Claim: " + claimAcknowledgementHeaderId + " Not Sent To RMQ queue: " + _queue.AntiLogForging() + " (Enqueuments are disabled in the app.config)");

                    // Json File Creation
                    if (_fileOutputJsonEnabled == "true") // SC: One file per Json message
                        DeleteExistingJsonFileAndCreateNewFile(claimAcknowledgementHeaderId, json);
                    //else
                    //    Log.Info("Claim: " + claimAcknowledgementHeaderId + " Not Sent To Json File Output (Json File Output is Disabled in the app.config)");
                        
                    //updatedSuccessfully = _fhirDal.UpdateClaimAcknowledgementHeaderClaimStatus(claimAcknowledgementHeaderId, FhirStatus.QUEUED);
                }
                catch (Exception ex)
                {
                    Log.Error("Error Sending Json for Claim " + claimAcknowledgementHeaderId, ex);
                    //updatedSuccessfully = _fhirDal.UpdateClaimAcknowledgementHeaderClaimStatus(claimAcknowledgementHeaderId, FhirStatus.NEW); //set back for new attempt
                }
            }
            
            //if (updatedSuccessfully == false)
            //{
            //    Log.Error("Was Not Able to Update the Claim Status for Claim: " + claimAcknowledgementHeaderId);
            //}
        }


        //////public void EnqueueJson(int claimAcknowledgmentHeaderId, string json)
        //////{
        //////    _rmqWrapper.Send(_queue, Encoding.UTF8.GetBytes(json));
        //////    //Log.Info($"Claim {claimAcknowledgmentHeaderId} Sent to {_queue.AntiLogForging()}");
        //////    //_f277PerformanceAnalysisService.UpdateF277EnqueuementTime(claimAcknowledgmentHeaderId, DateTime.Now);
        //////}

        public void DeleteExistingJsonFileAndCreateNewFile(int claimAcknowledgmentHeaderId, string json)
        {
            bool outputDirectoryCreatedSuccessfullyOrAlreadyExists = CreateJsonOutputDirectoryIfNecessary();
            DeleteExisting277JsonFileIfPresent(claimAcknowledgmentHeaderId);
            Create277JsonFile(claimAcknowledgmentHeaderId, _queue, json);
        }
                
        public bool CreateJsonOutputDirectoryIfNecessary()
        {
            bool outputDirectoryCreatedSuccessfullyOrAlreadyExists = false;
            // Create the output directory if it doesn't exist
            if (!Directory.Exists(_fileOutputJsonRootPath))
            {
                try
                {
                    Directory.CreateDirectory(CleanPath.CleanString(_fileOutputJsonRootPath));
                    outputDirectoryCreatedSuccessfullyOrAlreadyExists = true;
                }
                catch (Exception ex)
                {
                    Log.Error("Unable To Create 277 Json Output Directory " + ex.ToString());
                }
            }
            else
                outputDirectoryCreatedSuccessfullyOrAlreadyExists = true;

            return outputDirectoryCreatedSuccessfullyOrAlreadyExists;
        }

        public bool Create277JsonFile(int claimAcknowledgementHeaderId, string queueName, string json)
        {
            bool fileCreationWasSuccessful = false;
            try
            {
                var fileName = claimAcknowledgementHeaderId + "_Json.txt";
                var fullFilePath = _fileOutputJsonRootPath + fileName;

                using (StreamWriter sw = File.CreateText(CleanPath.CleanString(fullFilePath)))
                {
                    var header = "Queue Name: " + queueName + " ClaimAcknowledgementHeaderId: " + claimAcknowledgementHeaderId + " Date: " + DateTime.Now;
                    sw.WriteLine(header);
                    sw.WriteLine(json);
                }

                //_fhirDal.InsertFileHistoryTable(claimAcknowledgementHeaderId);
                fileCreationWasSuccessful = true;
                //Log.Info("Created 277 Json File for Claim: " + claimAcknowledgementHeaderId);
            }
            catch(Exception ex)
            {
                Log.Error("Unable to Create 277 Json File for Claim: " + claimAcknowledgementHeaderId, ex);
            }
            return fileCreationWasSuccessful;
        }

        public bool DeleteExisting277JsonFileIfPresent(int lastClaimAcknowledgmentHeaderId)
        {
            bool fileDeletedSuccessfullyOrDoesNotExist = false;
            var fileName = lastClaimAcknowledgmentHeaderId + "_Json.txt";
            var fullFilePath = CleanPath.CleanString(_fileOutputJsonRootPath + fileName);
            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    fileDeletedSuccessfullyOrDoesNotExist = true;
                }
                else
                    fileDeletedSuccessfullyOrDoesNotExist = true;
            }
            catch(Exception ex)
            {
                Log.Error("Unable to Delete Existing 277 Json File for Claim: " + lastClaimAcknowledgmentHeaderId,ex);
            }
            return fileDeletedSuccessfullyOrDoesNotExist;
        }

        public override void Dispose()
        {
            //Dispose(true);
        }
    }
}
