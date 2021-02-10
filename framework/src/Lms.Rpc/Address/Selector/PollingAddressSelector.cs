using System.Collections.Concurrent;
using System.Linq;
using Lms.Rpc.Address.HealthCheck;

namespace Lms.Rpc.Address.Selector
{
    public class PollingAddressSelector : AddressSelectorBase
    {
        private ConcurrentDictionary<string, (int, IAddressModel[])> addressesPools =
            new();

        private readonly IHealthCheck _healthCheck;

        public PollingAddressSelector(IHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            _healthCheck.OnRemveAddress += async addressMoel =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(addressMoel)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out var removeAdress);
                }
            };
            _healthCheck.OnUnhealth += async addressMoel =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(addressMoel)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out var removeAdress);
                }
            };
        }

        protected override IAddressModel SelectAddressByAlgorithm(AddressSelectContext context)
        {
            var selectAdderessItem = (0, context.AddressModels);
            var index = 0;
            if (addressesPools.ContainsKey(context.ServiceId))
            {
                selectAdderessItem = addressesPools.GetOrAdd(context.ServiceId, selectAdderessItem);
                index = selectAdderessItem.Item1 >= selectAdderessItem.Item2.Count(p => p.Enabled)
                    ? 0
                    : selectAdderessItem.Item1;
            }

            var enableAddress = selectAdderessItem.Item2.Where(p => p.Enabled).ToArray();
            var selectAdderess = enableAddress[index];
            selectAdderessItem.Item1 = index + 1;
            addressesPools.AddOrUpdate(context.ServiceId, selectAdderessItem, (k, v) => selectAdderessItem);
            return selectAdderess;
        }
    }
}