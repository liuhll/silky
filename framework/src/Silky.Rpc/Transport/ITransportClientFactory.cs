using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;

namespace Silky.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> GetClient(IRpcAddress rpcAddress);
    }
}