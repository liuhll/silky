using Consul;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public static class AgentServiceExtensions
    {
        public static ServerDescriptor GetServerDescriptor(this AgentService agentService)
        {
            var serverDescriptor = new ServerDescriptor()
            {
            };
            return serverDescriptor;
        }

        public static string GetServerName(this AgentService agentService)
        {
            return agentService.Meta["HostName"];
        }
    }
}