using System.Threading;
using System.Threading.Tasks;
using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;
using FSC.ServiceBus.Reception;
using FSC.ServiceBus.Contracts;
using Microsoft.Extensions.Logging;

namespace FSC.ServiceBus.Receiver.ServiceBus
{
    public class StringMessageHandler : IMessageReceptionHandler<string>
    {
        private readonly ILogger<StringMessageHandler> _logger;
        private readonly IMessageMetadataAccessor _metaData;

        public static bool ShouldFail { get; set; }
        public static bool ShouldAbandon { get; set; }

        public static bool ShouldComplete { get; set; }

        public StringMessageHandler(ILogger<StringMessageHandler> logger, IMessageMetadataAccessor metaData)
        {
            _logger = logger;
            _metaData = metaData;
        }

        public async Task Handle(string result, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received from queue : {result}");


            if (StringMessageHandler.ShouldFail)
            {
                _logger.LogInformation("Move message directly to the dead letter");
                await _metaData.Metadata.DeadLetterMessageAsync();
                return;
            }

            if (StringMessageHandler.ShouldAbandon)
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
