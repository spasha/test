using System;
using System.Linq;
using FSC.ServiceBus.Abstractions;

namespace FSC.ServiceBus.Reception
{

  [Serializable]
  public class MessageIsMissingPayloadTypeIdException : Exception
  {
    public MessageIsMissingPayloadTypeIdException(MessageContext messageContext)
    {
      Message = "An incoming message is missing its 'PayloadTypeId' UserProperty. Processing cannot continue without this information.\n"
                + $"context : \n"
                + $"\tReceiver : {messageContext.ClientType} {messageContext.ResourceId}"
                + $"\tLabel : {messageContext.Message.Subject}"
                + $"\tUser properties : \n"
                + $"{string.Join("\n", messageContext.Message.ApplicationProperties.Select(o => $"\t\t{o.Key} : {o.Value}"))}";
    }

    public override string Message { get; }
  }
}