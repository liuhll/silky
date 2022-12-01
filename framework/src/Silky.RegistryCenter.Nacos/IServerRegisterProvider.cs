using System.Collections.Generic;
using System.Threading.Tasks;
using Nacos.V2.Naming.Dtos;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServerRegisterProvider
    {
        Task AddServer();

        Task<IList<string>> GetAllServerNames(int timeoutMs = 10000);

        Task<ServerDescriptor> GetServerDescriptor(string serverName, List<Instance> serverInstances);
    }
}