using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Address;

namespace Silky.Lms.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> GetClient(IAddressModel address);
    }
}