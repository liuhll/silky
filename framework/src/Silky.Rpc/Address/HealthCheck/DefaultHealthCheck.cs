using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Address.HealthCheck
{
    public class DefaultHealthCheck : IHealthCheck
    {
        private ConcurrentDictionary<IAddressModel, HealthCheckModel> m_healthCheckAddresses = new();
        public event HealthChangeEvent OnHealthChange;
        public event RemoveAddressEvent OnRemveAddress;
        public event UnhealthEvent OnUnhealth;
        public event AddMonitorEvent OnAddMonitor;
        public event RemoveServiceRouteAddressEvent OnRemoveServiceRouteAddress;

        public void RemoveAddress(IAddressModel addressModel)
        {
            m_healthCheckAddresses.TryRemove(addressModel, out _);
            OnRemveAddress?.Invoke(addressModel);
        }

        public void RemoveServiceRouteAddress(string serviceId, IAddressModel addressModel)
        {
            m_healthCheckAddresses.TryRemove(addressModel, out _);
            OnRemoveServiceRouteAddress?.Invoke(serviceId, addressModel);
        }

        public void RemoveAddress(IPAddress ipAddress, int port)
        {
            var key = m_healthCheckAddresses.Keys.FirstOrDefault(p =>
                p.IPEndPoint.Address.MapToIPv4().Equals(ipAddress) && p.Port == port);
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
                OnAddMonitor?.Invoke(addressModel);
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

        public void ChangeHealthStatus(IPAddress ipAddress, int port, bool isHealth, int unHealthCeilingTimes = 0)
        {
            var addressModel = NetUtil.CreateAddressModel(ipAddress.ToString(), port, ServiceProtocol.Tcp);
            ChangeHealthStatus(addressModel, isHealth, unHealthCeilingTimes);
        }

        public void ChangeHealthStatus(IAddressModel addressModel, bool isHealth, int unHealthCeilingTimes = 0)
        {
            if (m_healthCheckAddresses.TryGetValue(addressModel, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new HealthCheckModel(isHealth, isHealth ? 0 : healthCheckModel.UnHealthTimes + 1);
                m_healthCheckAddresses.TryUpdate(addressModel, newHealthCheckModel, healthCheckModel);
                healthCheckModel = newHealthCheckModel;
            }
            else
            {
                healthCheckModel = new HealthCheckModel(isHealth, isHealth ? 0 : 1);
                m_healthCheckAddresses.TryAdd(addressModel, healthCheckModel);
            }

            if (!isHealth && healthCheckModel.UnHealthTimes >= unHealthCeilingTimes)
            {
                OnRemveAddress?.Invoke(addressModel);
                m_healthCheckAddresses.TryRemove(addressModel, out var value);
            }

            if (healthCheckModel.IsHealth != isHealth)
            {
                OnHealthChange?.Invoke(addressModel, isHealth);
            }

            if (!isHealth)
            {
                OnUnhealth?.Invoke(addressModel);
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