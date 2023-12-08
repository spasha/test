using FSC.ServiceBus.Abstractions;

namespace FSC.ServiceBus.Dispatch
{
  public class MessageContext : IMessageContext
  {
    public string CorrelationId { get; set; }
    public string SessionId { get; set; }
  }
}