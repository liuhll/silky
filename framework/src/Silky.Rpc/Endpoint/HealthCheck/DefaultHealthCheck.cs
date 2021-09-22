using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Silky.Core.Rpc;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Endpoint.HealthCheck
{
    public class DefaultHealthCheck : IHealthCheck
    {
        private ConcurrentDictionary<IRpcEndpoint, HealthCheckModel> m_healthCheckEndpoints = new();
        public event HealthChangeEvent OnHealthChange;
        public event RemoveRpcEndpointEvent OnRemoveRpcEndpoint;
        public event UnhealthEvent OnUnhealth;
        public event AddMonitorEvent OnAddMonitor;

        public void RemoveRpcEndpoint(IRpcEndpoint rpcEndpoint)
        {
            m_healthCheckEndpoints.TryRemove(rpcEndpoint, out _);
            OnRemoveRpcEndpoint?.Invoke(rpcEndpoint);
        }

        public void RemoveRpcEndpoint(IPAddress ipAddress, int port)
        {
            var key = m_healthCheckEndpoints.Keys.FirstOrDefault(p =>
                p.IPEndPoint.Address.MapToIPv4().Equals(ipAddress) && p.Port == port);
            if (key != null)
            {
                RemoveRpcEndpoint(key);
            }
        }

        public void Monitor(IRpcEndpoint rpcEndpoint)
        {
            if (!m_healthCheckEndpoints.ContainsKey(rpcEndpoint))
            {
                m_healthCheckEndpoints.GetOrAdd(rpcEndpoint, new HealthCheckModel(true, 0));
                OnAddMonitor?.Invoke(rpcEndpoint);
            }
        }

        public bool IsHealth(IPEndPoint ipEndpoint)
        {
            var key = m_healthCheckEndpoints.Keys.FirstOrDefault(p => p.IPEndPoint.Equals(ipEndpoint));
            if (key != null)
            {
                return IsHealth(key);
            }

            return false;
        }

        public bool IsHealth(RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            var key = m_healthCheckEndpoints.Keys.FirstOrDefault(p => p.Descriptor == rpcEndpointDescriptor);
            if (key != null)
            {
                return IsHealth(key);
            }

            return false;
        }

        public bool IsHealth(IRpcEndpoint rpcEndpoint)
        {
            if (m_healthCheckEndpoints.TryGetValue(rpcEndpoint, out var checkModel))
            {
                return checkModel.IsHealth;
            }

            return false;
        }

        public void ChangeHealthStatus(IPAddress ipAddress, int port, ServiceProtocol serviceProtocol, bool isHealth,
            int unHealthCeilingTimes = 0)
        {
            var rpcEndpoint = RpcEndpointHelper.CreateRpcEndpoint(ipAddress.ToString(), port, serviceProtocol);
            ChangeHealthStatus(rpcEndpoint, isHealth, unHealthCeilingTimes);
        }

        public void ChangeHealthStatus(IRpcEndpoint rpcEndpoint, bool isHealth, int unHealthCeilingTimes = 0)
        {
            if (m_healthCheckEndpoints.TryGetValue(rpcEndpoint, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new HealthCheckModel(isHealth, isHealth ? 0 : healthCheckModel.UnHealthTimes + 1);
                m_healthCheckEndpoints.TryUpdate(rpcEndpoint, newHealthCheckModel, healthCheckModel);
                healthCheckModel = newHealthCheckModel;
            }
            else
            {
                healthCheckModel = new HealthCheckModel(isHealth, isHealth ? 0 : 1);
                m_healthCheckEndpoints.TryAdd(rpcEndpoint, healthCheckModel);
            }

            if (!isHealth && healthCheckModel.UnHealthTimes >= unHealthCeilingTimes)
            {
                OnRemoveRpcEndpoint?.Invoke(rpcEndpoint);
                m_healthCheckEndpoints.TryRemove(rpcEndpoint, out var value);
            }

            if (healthCheckModel.IsHealth != isHealth)
            {
                OnHealthChange?.Invoke(rpcEndpoint, isHealth);
            }

            if (!isHealth)
            {
                OnUnhealth?.Invoke(rpcEndpoint);
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