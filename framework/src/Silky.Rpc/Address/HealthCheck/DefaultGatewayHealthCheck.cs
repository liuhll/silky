using System;
using System.Collections.Concurrent;
using System.Threading;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Address.HealthCheck
{
    public class DefaultGatewayHealthCheck : IGatewayHealthCheck, IDisposable
    {
        private ConcurrentDictionary<IAddressModel, HealthCheckModel> m_healthCheckAddresses = new();

        public event RemoveAddressEvent OnRemveAddress;
        public event UnhealthEvent OnUnhealth;
        public event AddMonitorEvent OnAddMonitor;

        private Timer _healthTimer;

        public DefaultGatewayHealthCheck()
        {
            _healthTimer = new Timer(HealthCheckTask, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
        }

        private void HealthCheckTask(object? state)
        {
            foreach (var checkAddress in m_healthCheckAddresses)
            {
                if (SocketCheck.TestConnection(checkAddress.Key.Address, checkAddress.Key.Port))
                {
                    checkAddress.Value.IsHealth = true;
                    checkAddress.Value.UnHealthTimes = 0;
                }
                else
                {
                    checkAddress.Value.IsHealth = false;
                    OnUnhealth?.Invoke(checkAddress.Key);
                    checkAddress.Value.UnHealthTimes += 1;
                }

                m_healthCheckAddresses[checkAddress.Key] = checkAddress.Value;
                if (checkAddress.Value.UnHealthTimes > 3)
                {
                    OnRemveAddress?.Invoke(checkAddress.Key);
                }
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

        public void Dispose()
        {
            _healthTimer?.Dispose();
        }
    }
}