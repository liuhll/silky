using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Gateway.Descriptor;

namespace Silky.Rpc.Gateway
{
    public class GatewayCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, GatewayDescriptor> _gatewayDescriptorCache = new();

        public ILogger<GatewayCache> Logger { get; set; }

        private readonly IGatewayHealthCheck _healthCheck;

        public GatewayCache(IGatewayHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            _healthCheck.OnRemveAddress += async addressModel =>
            {
                var gatewayManager = EngineContext.Current.Resolve<IGatewayManager>();
                var removeAddressDescriptors =
                    _gatewayDescriptorCache.Values.Where(p => p.Addresses.Any(p => p == addressModel.Descriptor));
                foreach (var removeAddressDescriptor in removeAddressDescriptors)
                {
                    await gatewayManager.RemoveGatewayAddress(removeAddressDescriptor.HostName, addressModel);
                }
            };
        }

        public void UpdateCache([NotNull] GatewayDescriptor gatewayDescriptor)
        {
            Check.NotNull(gatewayDescriptor, nameof(gatewayDescriptor));
            _gatewayDescriptorCache.AddOrUpdate(gatewayDescriptor.HostName, gatewayDescriptor,
                (k, v) => gatewayDescriptor);
            foreach (var addressDescriptor in gatewayDescriptor.Addresses)
            {
                _healthCheck.Monitor(addressDescriptor.ConvertToAddressModel());
            }
        }

        public void RemoveCache(string hostName)
        {
            Check.NotNull(hostName, nameof(hostName));
            _gatewayDescriptorCache.TryRemove(hostName, out _);
        }

        public IReadOnlyCollection<GatewayDescriptor> Gateways =>
            _gatewayDescriptorCache.Select(p => p.Value).ToArray();
    }
}