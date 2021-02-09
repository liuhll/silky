using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Address.HealthCheck
{
    public class DefaultHealthCheck : IHealthCheck
    {
        private ConcurrentDictionary<IAddressModel, HealthCheckModel> m_healthCheckAddresses = new();
        private readonly RpcOptions _rpcOptions;
        public event HealthChangeEvent OnHealthChange;
        public event RemoveAddressEvent OnRemveAddress;
        public event UnhealthEvent OnUnhealth;
        public DefaultHealthCheck(IOptions<RpcOptions> rpcOptions)
        {
            _rpcOptions = rpcOptions.Value;
        }


        public void RemoveAddress(IAddressModel addressModel)
        {
            if (m_healthCheckAddresses.TryGetValue(addressModel, out var val))
            {
                m_healthCheckAddresses.TryRemove(addressModel, out val);
                OnRemveAddress?.Invoke(addressModel);
            }
        }

        public void RemoveAddress(IPAddress ipAddress, int port)
        {
            var key = m_healthCheckAddresses.Keys.FirstOrDefault(p => p.IPEndPoint.Address.MapToIPv4().Equals(ipAddress) && p.Port == port);
            if (key != null)
            {
                RemoveAddress(key);
            }
        }

        public void Monitor(IAddressModel addressModel)
        {
            if (!m_healthCheckAddresses.ContainsKey(addressModel))
            {
                m_healthCheckAddresses.GetOrAdd(addressModel, new HealthCheckModel(true, 0));
            }
        }
        
        public bool IsHealth(IPEndPoint ipEndpoint)
        {
            var key = m_healthCheckAddresses.Keys.FirstOrDefault(p => p.IPEndPoint.Equals(ipEndpoint));
            if (key != null)
            {
                return IsHealth(key);
            }

            return false;
        }

        public bool IsHealth(AddressDescriptor addressDescriptor)
        {
            var key = m_healthCheckAddresses.Keys.FirstOrDefault(p => p.Descriptor == addressDescriptor);
            if (key != null)
            {
                return IsHealth(key);
            }

            return false;
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
                if (!isHealth && healthCheckModel.UnHealthTimes >= _rpcOptions.UnHealthCeilingTimes &&
                    OnRemveAddress != null)
                {
                    OnRemveAddress(addressModel);
                }

                if (healthCheckModel.IsHealth != isHealth && OnHealthChange != null)
                {
                    OnHealthChange(addressModel, isHealth);
                }
            }

            healthCheckModel = new HealthCheckModel(isHealth, isHealth ? 0 : 1);
            m_healthCheckAddresses.TryAdd(addressModel, healthCheckModel);
            if (!isHealth && OnUnhealth != null)
            {
                OnUnhealth(addressModel);
            }
        }

        private class HealthCheckModel
        {
            public HealthCheckModel(bool isHealth, int unHealthTimes)
            {
                IsHealth = isHealth;
                UnHealthTimes = unHealthTimes;
            }

            public bool IsHealth { get; set; }

            public int UnHealthTimes { get; set; }
        }
    }
}