using System.Net;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Address;

namespace Lms.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> GetClient(IAddressModel address);
    }
}