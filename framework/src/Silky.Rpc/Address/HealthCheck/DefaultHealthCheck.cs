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
        private ConcurrentDictionary<IRpcAddress, HealthCheckModel> m_healthCheckAddresses = new();
        public event HealthChangeEvent OnHealthChange;
        public event RemoveAddressEvent OnRemveAddress;
        public event UnhealthEvent OnUnhealth;
        public event AddMonitorEvent OnAddMonitor;

        public void RemoveAddress(IRpcAddress rpcAddress)
        {
            m_healthCheckAddresses.TryRemove(rpcAddress, out _);
            OnRemveAddress?.Invoke(rpcAddress);
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

        public void Monitor(IRpcAddress rpcAddress)
        {
            if (!m_healthCheckAddresses.ContainsKey(rpcAddress))
            {
                m_healthCheckAddresses.GetOrAdd(rpcAddress, new HealthCheckModel(true, 0));
                OnAddMonitor?.Invoke(rpcAddress);
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

        public bool IsHealth(IRpcAddress rpcAddress)
        {
            if (m_healthCheckAddresses.TryGetValue(rpcAddress, out var checkModel))
            {
                return checkModel.IsHealth;
            }

            return false;
        }

        public void ChangeHealthStatus(IPAddress ipAddress, int port, bool isHealth, int unHealthCeilingTimes = 0)
        {
            var addressModel = AddressHelper.CreateAddressModel(ipAddress.ToString(), port, ServiceProtocol.Tcp);
            ChangeHealthStatus(addressModel, isHealth, unHealthCeilingTimes);
        }

        public void ChangeHealthStatus(IRpcAddress rpcAddress, bool isHealth, int unHealthCeilingTimes = 0)
        {
            if (m_healthCheckAddresses.TryGetValue(rpcAddress, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new HealthCheckModel(isHealth, isHealth ? 0 : healthCheckModel.UnHealthTimes + 1);
                m_healthCheckAddresses.TryUpdate(rpcAddress, newHealthCheckModel, healthCheckModel);
                healthCheckModel = newHealthCheckModel;
            }
            else
            {
                healthCheckModel = new HealthCheckModel(isHealth, isHealth ? 0 : 1);
                m_healthCheckAddresses.TryAdd(rpcAddress, healthCheckModel);
            }

            if (!isHealth && healthCheckModel.UnHealthTimes >= unHealthCeilingTimes)
            {
                OnRemveAddress?.Invoke(rpcAddress);
                m_healthCheckAddresses.TryRemove(rpcAddress, out var value);
            }

            if (healthCheckModel.IsHealth != isHealth)
            {
                OnHealthChange?.Invoke(rpcAddress, isHealth);
            }

            if (!isHealth)
            {
                OnUnhealth?.Invoke(rpcAddress);
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