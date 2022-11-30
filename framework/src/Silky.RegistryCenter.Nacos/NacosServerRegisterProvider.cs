using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nacos.V2.Naming.Dtos;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServerRegisterProvider : IServerRegisterProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public NacosServerRegisterProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ServerDescriptor[]> GetServerDescriptor(List<Instance> serverInstances)
        {
            var serverDict = new Dictionary<string, ServerDescriptor>();
            foreach (var serverInstance in serverInstances)
            {
                var serverName = serverInstance.GetServerName();
                if (serverDict.TryGetValue(serverName, out var serverDescriptor))
                {
                    var instanceEndpoints = serverInstance.GetEndpoints();
                    serverDescriptor.Endpoints = serverDescriptor.Endpoints.Concat(instanceEndpoints).ToArray();
                }
                else
                {
                    var serviceDescriptors = await _serviceProvider.GetServices(serverName);
                    serverDescriptor = new ServerDescriptor()
                    {
                        HostName = serverName,
                        Services = serviceDescriptors,
                        Endpoints = serverInstance.GetEndpoints().ToArray(),
                    };
                    serverDict[serverName] = serverDescriptor;
                }
            }

            return serverDict.Values.ToArray();
        }
    }
}