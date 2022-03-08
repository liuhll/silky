using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Endpoint.Monitor
{
    public class DefaultRpcEndpointMonitor : IRpcEndpointMonitor
    {
        private ConcurrentDictionary<IRpcEndpoint, CheckModel> m_checkEndpoints = new();

        public event StatusChangeEvent OnStatusChange;
        public event RemoveRpcEndpointEvent OnRemoveRpcEndpoint;
        public event DisEnableEvent OnDisEnable;
        public event AddMonitorEvent OnAddMonitor;


        public void RemoveRpcEndpoint(IRpcEndpoint rpcEndpoint)
        {
            m_checkEndpoints.TryRemove(rpcEndpoint, out _);
            OnRemoveRpcEndpoint?.Invoke(rpcEndpoint);
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

        public void Monitor(IRpcEndpoint rpcEndpoint)
        {
            if (!m_checkEndpoints.ContainsKey(rpcEndpoint))
            {
                m_checkEndpoints.GetOrAdd(rpcEndpoint, new CheckModel(true, 0));
                OnAddMonitor?.Invoke(rpcEndpoint);
            }
        }


        public bool IsEnable(IRpcEndpoint rpcEndpoint)
        {
            if (m_checkEndpoints.TryGetValue(rpcEndpoint, out var checkModel))
            {
                return checkModel.IsEnable && rpcEndpoint.Enabled;
            }

            return false;
        }

        public void ChangeStatus(IPAddress ipAddress, int port, ServiceProtocol serviceProtocol, bool isEnable,
            int unHealthCeilingTimes = 0)
        {
            var rpcEndpoint = RpcEndpointHelper.CreateRpcEndpoint(ipAddress.ToString(), port, serviceProtocol);
            ChangeStatus(rpcEndpoint, isEnable, unHealthCeilingTimes);
        }

        public void ChangeStatus(IRpcEndpoint rpcEndpoint, bool isEnable, int unHealthCeilingTimes = 0)
        {
            if (m_checkEndpoints.TryGetValue(rpcEndpoint, out var healthCheckModel))
            {
                var newHealthCheckModel =
                    new CheckModel(isEnable, isEnable ? 0 : healthCheckModel.UnHealthTimes + 1);
                m_checkEndpoints.TryUpdate(rpcEndpoint, newHealthCheckModel, healthCheckModel);
                healthCheckModel = newHealthCheckModel;
            }
            else
            {
                healthCheckModel = new CheckModel(isEnable, isEnable ? 0 : 1);
                m_checkEndpoints.TryAdd(rpcEndpoint, healthCheckModel);
            }
            if (!isEnable && healthCheckModel.UnHealthTimes >= unHealthCeilingTimes)
            {
                OnRemoveRpcEndpoint?.Invoke(rpcEndpoint);
                m_checkEndpoints.TryRemove(rpcEndpoint, out var value);
            }

            if (healthCheckModel.IsEnable != isEnable)
            {
                OnStatusChange?.Invoke(rpcEndpoint, isEnable);
            }

            if (!isEnable)
            {
                OnDisEnable?.Invoke(rpcEndpoint);
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