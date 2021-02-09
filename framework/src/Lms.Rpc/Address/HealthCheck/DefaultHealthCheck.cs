using System.Collections.Concurrent;
using System.Net;
using Lms.Rpc.Address.Descriptor;

namespace Lms.Rpc.Address.HealthCheck
{
    public class DefaultHealthCheck : IHealthCheck
    {
        private ConcurrentDictionary<IAddressModel, HealthCheckModel> m_healthCheckAddresses =
            new ConcurrentDictionary<IAddressModel, HealthCheckModel>();

        public event HealthChange HealthChange;

        public event ReachUnHealthCeilingTimes ReachUnHealthCeilingTimes;

        public void Monitor(IAddressModel addressModel)
        {
            if (!m_healthCheckAddresses.ContainsKey(addressModel))
            {
                m_healthCheckAddresses.GetOrAdd(addressModel, new HealthCheckModel(true, 0));
            }
        }

        public bool IsHealth(IPEndPoint ipEndPoint)
        {
            return true;
        }

        public bool IsHealth(AddressDescriptor addressDescriptor)
        {
            return true;
        }

        public bool IsHealth(IAddressModel addressModel)
        {
            if (m_healthCheckAddresses.TryGetValue(addressModel, out var checkModel))
            {
                return checkModel.IsHealth;
            }

            return false;
        }

        public void ChangeHealthStatus(IAddressModel addressModel, bool isHealth)
        {
            if (m_healthCheckAddresses.TryGetValue(addressModel, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new HealthCheckModel(isHealth, isHealth ? 0 : healthCheckModel.UnHealthTimes + 1);

                m_healthCheckAddresses.TryUpdate(addressModel, newHealthCheckModel, healthCheckModel);
            }

            healthCheckModel = new HealthCheckModel(isHealth, isHealth ? 0 : 1);
            m_healthCheckAddresses.TryAdd(addressModel, healthCheckModel);
        }

        private class HealthCheckModel
        {
            public HealthCheckModel(bool isHealth, int unHealthTimes)
            {
                IsHealth = isHealth;
                UnHealthTimes = unHealthTimes;
            }

            public bool IsHealth { get; internal set; }

            public int UnHealthTimes { get; internal set; }
        }
    }
}