using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Endpoint.Monitor
{
    public class DefaultRpcEndpointMonitor : IRpcEndpointMonitor
    {
        private ConcurrentDictionary<ISilkyEndpoint, CheckModel> m_checkEndpoints = new();

        public event StatusChangeEvent OnStatusChange;
        public event RemoveSilkyEndpointEvent OnRemoveRpcEndpoint;
        public event DisEnableEvent OnDisEnable;
        public event AddMonitorEvent OnAddMonitor;


        public void RemoveRpcEndpoint(ISilkyEndpoint silkyEndpoint)
        {
            m_checkEndpoints.TryRemove(silkyEndpoint, out _);
            OnRemoveRpcEndpoint?.Invoke(silkyEndpoint);
        }

        public void RemoveRpcEndpoint(IPAddress ipAddress, int port)
        {
            var key = m_checkEndpoints.Keys.FirstOrDefault(p =>
                p.IPEndPoint.Address.MapToIPv4().Equals(ipAddress) && p.Port == port);
            if (key != null)
            {
                RemoveRpcEndpoint(key);
            }
        }

        public void Monitor(ISilkyEndpoint silkyEndpoint)
        {
            if (!m_checkEndpoints.ContainsKey(silkyEndpoint))
            {
                m_checkEndpoints.GetOrAdd(silkyEndpoint, new CheckModel(true, 0));
                OnAddMonitor?.Invoke(silkyEndpoint);
            }
        }


        public bool IsEnable(ISilkyEndpoint silkyEndpoint)
        {
            if (m_checkEndpoints.TryGetValue(silkyEndpoint, out var checkModel))
            {
                return checkModel.IsEnable && silkyEndpoint.Enabled;
            }

            return false;
        }

        public void ChangeStatus(IPAddress ipAddress, int port, ServiceProtocol serviceProtocol, bool isEnable,
            int unHealthCeilingTimes = 0)
        {
            var rpcEndpoint = SilkyEndpointHelper.CreateRpcEndpoint(ipAddress.ToString(), port, serviceProtocol);
            ChangeStatus(rpcEndpoint, isEnable, unHealthCeilingTimes);
        }

        public void ChangeStatus(ISilkyEndpoint silkyEndpoint, bool isEnable, int unHealthCeilingTimes = 0)
        {
            if (m_checkEndpoints.TryGetValue(silkyEndpoint, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new CheckModel(isEnable, isEnable ? 0 : healthCheckModel.UnHealthTimes + 1);
                m_checkEndpoints.TryUpdate(silkyEndpoint, newHealthCheckModel, healthCheckModel);
                healthCheckModel = newHealthCheckModel;
            }
            else
            {
                healthCheckModel = new CheckModel(isEnable, isEnable ? 0 : 1);
                m_checkEndpoints.TryAdd(silkyEndpoint, healthCheckModel);
            }
            if (!isEnable && healthCheckModel.UnHealthTimes >= unHealthCeilingTimes)
            {
                OnRemoveRpcEndpoint?.Invoke(silkyEndpoint);
                m_checkEndpoints.TryRemove(silkyEndpoint, out var value);
            }

            if (healthCheckModel.IsEnable != isEnable)
            {
                OnStatusChange?.Invoke(silkyEndpoint, isEnable);
            }

            if (!isEnable)
            {
                OnDisEnable?.Invoke(silkyEndpoint);
            }
        }

        private class CheckModel
        {
            public CheckModel(bool isEnable, int unHealthTimes)
            {
                IsEnable = isEnable;
                UnHealthTimes = unHealthTimes;
            }

            public bool IsEnable { get; set; }

            public int UnHealthTimes { get; set; }
        }
    }
}