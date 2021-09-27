using System.Threading.Tasks;
using Consul;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public interface IServerConverter
    {
        Task<ServerDescriptor> Convert(string serverName, AgentService[] agentServices);
    }
}