using System.Collections.Generic;
using System.Threading.Tasks;
using Nacos.V2.Naming.Dtos;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public interface IServerRegisterProvider
    {
        
        Task<ServerDescriptor[]> GetServerDescriptor(List<Instance> serverInstances);
    }
}