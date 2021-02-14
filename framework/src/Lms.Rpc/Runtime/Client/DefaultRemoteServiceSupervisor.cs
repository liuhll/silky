using System;
using System.Collections.Concurrent;
using System.Linq;
using Lms.Rpc.Address;
using Lms.Rpc.Address.HealthCheck;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceSupervisor : IRemoteServiceSupervisor
    {
        private ConcurrentDictionary<(string, IAddressModel), ServiceInvokeInfo> m_monitor = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        public ILogger<DefaultRemoteServiceSupervisor> Logger { get; set; }


        public DefaultRemoteServiceSupervisor(IHealthCheck healthCheck,
            IServiceEntryLocator serviceEntryLocator)
        {
            _healthCheck = healthCheck;
            _serviceEntryLocator = serviceEntryLocator;
            _healthCheck.OnHealthChange += async (model, health) =>
            {
                if (!health)
                {
                    var keys = m_monitor.Keys.Where(p => p.Item2.Equals(model));
                    foreach (var key in keys)
                    {
                        var serviceEntry = _serviceEntryLocator.GetServiceEntryById(key.Item1);
                        key.Item2.MakeFusing(serviceEntry.GovernanceOptions.FuseSleepDuration);
                    }
                }
            };
            _healthCheck.OnRemveAddress += async model =>
            {
                var keys = m_monitor.Keys.Where(p => p.Item2.Equals(model));
                foreach (var key in keys)
                {
                    m_monitor.TryRemove(key, out var value);
                }
            };
            Logger = NullLogger<DefaultRemoteServiceSupervisor>.Instance;
        }

        public void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            if (serviceInvokeInfo.ConcurrentRequests > governanceOptions.MaxConcurrent)
            {
                item.Item2.MakeFusing(governanceOptions.FuseSleepDuration);
                Logger.LogWarning(
                    $"服务Id{item.Item1}--请求的地址{item.Item2}超过允许大最大并发量{governanceOptions.MaxConcurrent},当前并发量为{serviceInvokeInfo.ConcurrentRequests}");
                throw new OverflowException(
                    $"服务Id{item.Item1}--请求的地址{item.Item2}超过允许大最大并发量{governanceOptions.MaxConcurrent},当前并发量为{serviceInvokeInfo.ConcurrentRequests}");
            }

            serviceInvokeInfo.ConcurrentRequests++;
            serviceInvokeInfo.TotalRequests++;
            serviceInvokeInfo.FinalInvokeTime = DateTime.Now;
        }

        public void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            serviceInvokeInfo.ConcurrentRequests--;
            serviceInvokeInfo.AET = serviceInvokeInfo.AET.HasValue
                ? (serviceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                : elapsedTotalMilliseconds;
            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }

        public void ExceFail((string, IAddressModel) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            serviceInvokeInfo.ConcurrentRequests--;
            serviceInvokeInfo.FaultRequests++;
            serviceInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }
    }
}