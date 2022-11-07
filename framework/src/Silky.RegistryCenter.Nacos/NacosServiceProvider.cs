using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.RegistryCenter.Nacos.Listeners;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServiceProvider : IServiceProvider
    {
        private readonly ISerializer _serializer;
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;

        private readonly INacosConfigService _nacosConfigService;
        private ConcurrentDictionary<string, ServiceDescriptor[]> m_services = new();


        public NacosServiceProvider(ISerializer serializer,
            IOptionsMonitor<NacosRegistryCenterOptions> nacosRegistryCenterOptions,
            INacosConfigService nacosConfigService)
        {
            _serializer = serializer;

            _nacosConfigService = nacosConfigService;
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.CurrentValue;
        }

        public async Task<ServiceDescriptor[]> GetServices(string hostName, long timeoutMs = 10000)
        {
            if (m_services.TryGetValue(hostName, out var serviceDescriptors))
            {
                return serviceDescriptors;
            }

            var serviceConfigValue =
                await _nacosConfigService.GetConfigAndSignListener(hostName, _nacosRegistryCenterOptions.ServerGroupName,
                    timeoutMs,
                    new ServiceListener(hostName, this));
            if (serviceConfigValue.IsNullOrEmpty())
            {
                throw new SilkyException($"Failed to obtain the serviceDescriptor information of {hostName}");
            }

            serviceDescriptors = _serializer.Deserialize<ServiceDescriptor[]>(serviceConfigValue);
            m_services.TryAdd(hostName, serviceDescriptors);
            return serviceDescriptors;
        }

        public async Task PublishServices(string hostName, ServiceDescriptor[] serviceDescriptors)
        {
            if (m_services.TryGetValue(hostName, out var cacheServiceDescriptors))
            {
                if (serviceDescriptors.All(p => cacheServiceDescriptors.Any(q => p == q)))
                {
                    return;
                }
            }
            
            var serviceConfigValue = _serializer.Serialize(serviceDescriptors);
            var result = await _nacosConfigService.PublishConfig(hostName, _nacosRegistryCenterOptions.ServerGroupName,
                serviceConfigValue);
            if (!result)
            {
                throw new SilkyException($"Failed to publish {hostName} service description information");
            }
        }

        public void UpdateService(string hostName, string configInfo)
        {
            var serviceDescriptors = _serializer.Deserialize<ServiceDescriptor[]>(configInfo);
            m_services.AddOrUpdate(hostName, serviceDescriptors, (k, v) => serviceDescriptors);
        }
    }
}