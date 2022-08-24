using System.Threading.Tasks;
using Silky.Rpc.Transport;

namespace Silky.DotNetty.Abstraction;

public interface ITransportClientPool
{
    Task<ITransportClient> GetOrCreate();
}