using System.Collections.Generic;
using System.Threading.Tasks;
using Nacos.V2.Naming.Dtos;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServerRegisterProvider
    {
        Task AddServer();

        Task<string[]> GetAllServerNames(int timeoutMs = 5000);

        ServerDescriptor GetServerDescriptor(string serverName, List<Instance> serverInstances,
            ServiceDescriptor[] serviceDescriptors);
    }
}