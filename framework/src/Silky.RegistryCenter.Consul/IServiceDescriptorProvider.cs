using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public interface IServiceDescriptorProvider
    {
        Task PublishAsync(string serverName, ServiceDescriptor[] serviceDescriptors);

        Task<ServiceDescriptor[]> GetServices(string serverName);
    }
}