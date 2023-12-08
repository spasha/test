using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using VA.FSC.Services.Common.Enums;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using FSC.ServiceBus.Receiver;
using FSC.ServiceBus;
using FSC.ServiceBus.Contracts;
using VA.EDI.F277.ServiceBus;
using FSC.ServiceBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace VA.FSC.Services
{
    public partial class FHIRProcessor : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessorBase));
        private readonly IService _service;

        List<IProcessor> processors;
        public interface IService : IDisposable
        {
            void Start();
        }
        public FHIRProcessor(IService service, string name)
        {
            _service = service;
            ServiceName = name;
        }

        public FHIRProcessor()
        {
            XmlConfigurator.Configure(); //log4net
            MDC.Set("machine", Environment.MachineName); // log4net
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("Starting FHIR Processor");
            Initialize();
            Log.Info($"Service: {base.ServiceName} Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} STARTED");
        }

        protected override void OnStop()
        {
            Log.Info("Service Stopping...");

            foreach (var p in processors)
            {
                p.Dispose();
            }

            Log.Info($"Service: {base.ServiceName} Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} STOPPED");
        }

        public void Initialize()
        {
            Log.Info("Looking for Processor Assemblies in the Application Root");
            try
            {

                var queueConnectionString = ConfigurationManager.AppSettings["ASBQueueConnectionString"];
                var clientOptions = new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                };

                var builder = Host.CreateDefaultBuilder();

                builder.ConfigureServices((context, services) =>
                {
                    services.AddServiceBus<StringMessagePayloadSerializer>(
                            settings =>
                            {
                                settings.WithConnection(queueConnectionString, clientOptions);
                            })
                        .RegisterEventListener<ServiceBusEventListener>();

                    //if you want to receive
                    services.RegisterServiceBusReception().FromQueue(ServiceBusResources.statReceipts, builder2 =>
                    {
                        builder2.RegisterReception<JsonModel, JsonModelHandler>();
                    });

                    //if you want to send
                    services.RegisterServiceBusDispatch().ToQueue(ServiceBusResources.statReceipts, builder2 =>
                    {
                        builder2.RegisterDispatch<JsonModel>(); ;
                    });
                });

                var host = builder.Build();
                host.RunAsync();

                try
                {
                    processors = new List<IProcessor>();
                    var assemblyList = new List<Assembly>();

                    // Scrape up the processors (837, 277) sitting in the application root and run them each in their own thread
                    var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

                    foreach (string dllFile in files)
                    {
                        assemblyList.Add(Assembly.LoadFile(dllFile));
                    }

                    var processorAssemblies = assemblyList.SelectMany(x => x.GetTypes()).Where(p => (typeof(IProcessor)).IsAssignableFrom(p));
                    foreach (var processorAssembly in processorAssemblies)
                    {
                        if (processorAssembly.Name != "IProcessor")
                        {
                            Log.Info("Found Processor Assembly: " + processorAssembly.Name);
                            var newProcessor = Activator.CreateInstance(processorAssembly, host, ConfigurationManager.ConnectionStrings["ConfigConnection"].ConnectionString);
                            processors.Add(newProcessor as IProcessor);
                        }
                    }

                    foreach (var processor in processors)
                    {
                        processor.ProcessingEvent += Processor_ProcessingEvent;
                        Task.Run(() =>
                        {
                            processor.RunProcess();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Critical Error Initializing Processors!", ex); // ErrorLevel.Critical
                }

                    //var service = host.Services.GetService<IDispatchSender>();






                    //////////processors = new List<IProcessor>();
                    //////////var assemblyList = new List<Assembly>();

                    //////////// Scrape up the processors (837, 277) sitting in the application root and run them each in their own thread
                    //////////var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

                    //////////foreach (string dllFile in files)
                    //////////{
                    //////////    if (dllFile.Contains("277"))
                    //////////        assemblyList.Add(Assembly.LoadFile(dllFile));
                    //////////}

                    ////////////////var processorAssemblies = assemblyList.SelectMany(x => x.GetTypes()).Where(p => (typeof(IProcessor)).IsAssignableFrom(p));
                    //////////var processorAssemblies = assemblyList.SelectMany(x => x.GetTypes()).Where(p => (typeof(IProcessor)).IsAssignableFrom(p));
                    //////////foreach (var processorAssembly in processorAssemblies)
                    //////////{
                    //////////    if (processorAssembly.Name != "IProcessor")
                    //////////    {
                    //////////        Log.Info("Found Processor Assembly: " + processorAssembly.Name);
                    //////////        var newProcessor = Activator.CreateInstance(processorAssembly,ConfigurationManager.ConnectionStrings["ConfigConnection"].ConnectionString);
                    //////////        processors.Add(newProcessor as IProcessor);
                    //////////    }
                    //////////}

                    //////////foreach (var processor in processors)
                    //////////{
                    //////////    processor.ProcessingEvent += Processor_ProcessingEvent;
                    //////////    Task.Run(() =>
                    //////////    {
                    //////////        processor.RunProcess();
                    //////////    });
                    //////////}
                }
            catch (Exception ex)
            {
                Log.Error("Critical Error Initializing Processors!", ex); // ErrorLevel.Critical
            }
        }

        private void Processor_ProcessingEvent(object sender, ProcessorEventArgs e)
        {
            if (e.EventLevel > ErrorLevel.Critical)
            {
                Log.Error("Error reported from Processor_ProcessingEvent, eventLevel = " + e.EventLevel.ToString());
            }
        }
    }
}
