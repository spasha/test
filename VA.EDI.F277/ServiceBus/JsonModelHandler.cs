using System.Threading;
using System.Threading.Tasks;
using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;
using FSC.ServiceBus.Reception;
using FSC.ServiceBus.Contracts;
using Microsoft.Extensions.Logging;

namespace VA.EDI.F277.ServiceBus //FSC.ServiceBus.Receiver.ServiceBus //VA.EDI.F277.ServiceBus
{
    public class JsonModelHandler : IMessageReceptionHandler<JsonModel>
    {
        private readonly ILogger<JsonModelHandler> _logger;
        private readonly IMessageMetadataAccessor _metaData;

        public static bool ShouldFail { get; set; }
        public static bool ShouldAbandon { get; set; }

        public static bool ShouldComplete { get; set; }

        public JsonModelHandler(ILogger<JsonModelHandler> logger, IMessageMetadataAccessor metaData)
        {
            _logger = logger;
            _metaData = metaData;
        }

        public async Task Handle(JsonModel result, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received from queue : {result.Name}");


            if (JsonModelHandler.ShouldFail)
            {
                _logger.LogInformation("Move message directly to the dead letter");
                await _metaData.Metadata.DeadLetterMessageAsync();
                return;
            }

            if (JsonModelHandler.ShouldAbandon)
            {
                _logger.LogInformation($"Abandon means message will appear in the queue again until max count reached");
                await _metaData.Metadata.AbandonMessageAsync();
                return;
            }

            _logger.LogInformation($"Complete Message after readling");
            await _metaData.Metadata.CompleteMessageAsync();
            return;
        }
    }
}

