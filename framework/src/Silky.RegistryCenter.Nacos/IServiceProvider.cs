using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServiceProvider
    {
        Task<ServiceDescriptor[]> GetServices(string hostName, long timeoutMs = 10000);

        Task PublishServices(string hostName, ServiceDescriptor[] serviceDescriptors);

        void UpdateService(string hostName, string configInfo);
    }
}