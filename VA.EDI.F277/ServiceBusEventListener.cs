using System;
using System.Threading.Tasks;
using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;
using log4net;
using VA.FSC.Services;
using VA.FSC.Services.Common.Helpers;
using FHIR.DAL;

namespace FSC.ServiceBus.Receiver
{
    public class ServiceBusEventListener : IServiceBusEventListener
    {
        private readonly IMessageMetadataAccessor _metaData;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessorBase));
        private FhirDal _fhirDal;
        private string claimId = string.Empty;
        private string timeStamp = string.Empty;


        public ServiceBusEventListener(IMessageMetadataAccessor metaData)
        {
             _metaData = metaData;
    }
        public Task OnExecutionStart(ExecutionStartedArgs args)
        {
            bool success = false;
            string deadLetterDescription = string.Empty;
            success = HelperMethods.Parse277Json(_metaData.Metadata.MessageBody, out claimId, out timeStamp);
            //success = HelperMethods.JsonParser.Get277StatReceiptAttributes(_metaData.Metadata.MessageBody, out claimId, out timeStamp);
            if (success)
            {
                _fhirDal = new FhirDal();
                success = _fhirDal.InsertSTATReceipts(claimId, timeStamp);
                if (success)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    deadLetterDescription = "Unable to Insert message into DB.";
                    _metaData.Metadata.DeadLetterMessageAsync();
                }
            }
            else
            {
                deadLetterDescription = "Unable to Parse 277 JSON File.";
                _metaData.Metadata.DeadLetterMessageAsync();
            }
            return Task.CompletedTask;
        }

        public Task OnExecutionSuccess(ExecutionSucceededArgs args)
        {
            Console.WriteLine(args.MessageLabel + "executed succesfully. Called after Handler is done");
            return Task.CompletedTask;
        }

        public Task OnExecutionFailed(ExecutionFailedArgs args)
        {
            Console.WriteLine(args.MessageLabel + "failed to execute. Called in catch block of entire processing.");
            return Task.CompletedTask;
        }
    }
}
