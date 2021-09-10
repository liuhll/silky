using System.Collections.Concurrent;
using System.Linq;
using Silky.Rpc.Address.HealthCheck;

namespace Silky.Rpc.Address.Selector
{
    public class PollingAddressSelector : AddressSelectorBase
    {
        private ConcurrentDictionary<string, (int, IAddressModel[])> addressesPools =
            new();

        private readonly IHealthCheck _healthCheck;

        public PollingAddressSelector(IHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            _healthCheck.OnRemveAddress += async addressModel =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(addressModel)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };

            _healthCheck.OnUnhealth += async addressMoel =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(addressMoel)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };
        }

        public override AddressSelectorMode AddressSelectorMode { get; } = AddressSelectorMode.Polling;

        protected override IAddressModel SelectAddressByAlgorithm(AddressSelectContext context)
        {
            var selectAdderessItem = (0, context.AddressModels);
            var index = 0;
            if (addressesPools.ContainsKey(context.MonitorId))
            {
                selectAdderessItem = addressesPools.GetOrAdd(context.MonitorId, selectAdderessItem);
                index = selectAdderessItem.Item1 >= selectAdderessItem.Item2.Count(p => p.Enabled)
                    ? 0
                    : selectAdderessItem.Item1;
            }

            var enableAddress = selectAdderessItem.Item2.Where(p => p.Enabled).ToArray();
            var selectAdderess = enableAddress[index];
            selectAdderessItem.Item1 = index + 1;
            addressesPools.AddOrUpdate(context.MonitorId, selectAdderessItem, (k, v) => selectAdderessItem);
            return selectAdderess;
        }
    }
}