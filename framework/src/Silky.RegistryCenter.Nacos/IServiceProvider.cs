using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServiceProvider
    {
        Task<ServiceDescriptor[]> GetServices(string hostName, long timeoutMs = 5000);

        Task PublishServices(string hostName, ServiceDescriptor[] serviceDescriptors,
            long timeoutMs = 5000);

        void UpdateService(string hostName, string configInfo);
    }
}