using Consul;
using Microsoft.Extensions.Options;
using Silky.RegistryCenter.Consul.Configuration;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulClientFactory : IConsulClientFactory
    {
        private readonly IOptionsMonitor<ConsulRegistryCenterOptions> _optionsMonitor;

        public ConsulClientFactory(IOptionsMonitor<ConsulRegistryCenterOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public IConsulClient CreateClient()
        {
            return new ConsulClient(_optionsMonitor.CurrentValue);
        }
    }
}
