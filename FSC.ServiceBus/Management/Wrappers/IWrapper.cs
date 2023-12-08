using System.Threading;
using System.Threading.Tasks;
using FSC.ServiceBus.Abstractions;

namespace FSC.ServiceBus
{

  public interface IWrapper
  {
    string ResourceId { get; }
    ClientType ClientType { get; }
    Task CloseAsync(CancellationToken cancellationToken);
  }
}
