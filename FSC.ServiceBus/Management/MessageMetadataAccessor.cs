using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;

namespace FSC.ServiceBus.Management {

  public class MessageMetadataAccessor : IMessageMetadataAccessor
  {
    public IMessageMetadata Metadata { get; private set; }

    public void SetData(MessageContext context)
    {
      if (context.SessionArgs != null)
      {
        Metadata = new MessageMetadata(context.Message, context.SessionArgs, context.CancellationToken);
      }
      else
      {
        Metadata = new MessageMetadata(context.Message, context.Args, context.CancellationToken);
      }
    }
  }
}