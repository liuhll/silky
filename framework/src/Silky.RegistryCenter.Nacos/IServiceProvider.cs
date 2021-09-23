using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServiceProvider
    {
        Task<ServiceDescriptor[]> GetServices(string hostName, string group, long timeoutMs = 1000);

        Task PublishServices(string hostName, string group, ServiceDescriptor[] serviceDescriptors,
            long timeoutMs = 1000);
    }
}