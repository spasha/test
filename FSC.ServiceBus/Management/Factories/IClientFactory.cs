using Azure.Messaging.ServiceBus;
using FSC.ServiceBus.Abstractions;

// ReSharper disable once CheckNamespace
namespace FSC.ServiceBus
{

  public interface IClientFactory
  {
    ServiceBusClient Create(ConnectionSettings connectionSettings);
  }
}