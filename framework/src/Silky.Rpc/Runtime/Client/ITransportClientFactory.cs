using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;

namespace Silky.Rpc.Runtime.Client
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> GetClient(IAddressModel address);
    }
}