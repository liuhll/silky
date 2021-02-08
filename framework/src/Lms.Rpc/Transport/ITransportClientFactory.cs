using System.Net;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);
    }
}