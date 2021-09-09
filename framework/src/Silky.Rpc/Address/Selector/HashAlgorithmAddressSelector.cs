using System.Collections.Concurrent;
using System.Linq;
using Silky.Core;
using Silky.Rpc.Address.HealthCheck;

namespace Silky.Rpc.Address.Selector
{
    public class HashAlgorithmAddressSelector : AddressSelectorBase
    {
        private ConcurrentDictionary<string, ConsistentHash<IAddressModel>> _consistentHashAddressPools = new();

        private readonly IHealthCheck _healthCheck;

        public HashAlgorithmAddressSelector(IHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            _healthCheck.OnRemveAddress += async addressModel =>
            {
                var removeItems = _consistentHashAddressPools
                    .Where(p => p.Value.ContainNode(addressModel))
                    .Select(p => p.Value);
                foreach (var consistentHash in removeItems)
                {
                    consistentHash.Remove(addressModel);
                }
            };

            _healthCheck.OnRemoveServiceRouteAddress += async (serviceId, addressModel) =>
            {
                var removeItems = _consistentHashAddressPools
                    .Where(p => p.Value.ContainNode(addressModel) && p.Key.Contains(serviceId))
                    .Select(p => p.Value);
                foreach (var consistentHash in removeItems)
                {
                    consistentHash.Remove(addressModel);
                }
            };

            _healthCheck.OnHealthChange += async (addressModel, isHealth) =>
            {
                var changeItems = _consistentHashAddressPools
                    .Where(p => p.Value.ContainNode(addressModel))
                    .Select(p => p.Value);
                foreach (var consistentHash in changeItems)
                {
                    if (!isHealth)
                    {
                        consistentHash.Remove(addressModel);
                    }
                    else
                    {
                        consistentHash.Add(addressModel);
                    }
                }
            };
        }

        public override AddressSelectorMode AddressSelectorMode { get; } = AddressSelectorMode.HashAlgorithm;

        protected override IAddressModel SelectAddressByAlgorithm(AddressSelectContext context)
        {
            Check.NotNullOrEmpty(context.Hash, nameof(context.Hash));
            var addressModels = _consistentHashAddressPools.GetOrAdd(context.ServiceEntryId, v =>
            {
                var consistentHash = new ConsistentHash<IAddressModel>();
                foreach (var address in context.AddressModels)
                {
                    consistentHash.Add(address);
                }

                return consistentHash;
            });
            if (addressModels.GetNodeCount() < context.AddressModels.Length)
            {
                foreach (var addressModel in context.AddressModels)
                {
                    if (!addressModels.ContainNode(addressModel))
                    {
                        addressModels.Add(addressModel);
                    }
                }
            }

            return addressModels.GetItemNode(context.Hash);
        }
    }
}