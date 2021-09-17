using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRequestServiceSupervisor : IRequestServiceSupervisor
    {
        private ConcurrentDictionary<(string, IAddressModel), ServiceInvokeInfo> m_monitor = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        public ILogger<DefaultRequestServiceSupervisor> Logger { get; set; }


        public DefaultRequestServiceSupervisor(IHealthCheck healthCheck,
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
                        key.Item2.MakeFusing(serviceEntry.GovernanceOptions.AddressFuseSleepDurationSeconds);
                    }
                }
            };
            _healthCheck.OnRemveAddress += async model =>
            {
                var keys = m_monitor.Keys.Where(p => p.Item2.Equals(model));
                foreach (var key in keys)
                {
                    m_monitor.TryRemove(key, out var _);
                }
            };

            _healthCheck.OnRemoveServiceRouteAddress += async (serviceId, model) =>
            {
                m_monitor.TryRemove((serviceId, model), out _);
            };
            Logger = NullLogger<DefaultRequestServiceSupervisor>.Instance;
        }

        public void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            serviceInvokeInfo.ConcurrentRequests++;
            serviceInvokeInfo.TotalRequests++;
            serviceInvokeInfo.FinalInvokeTime = DateTime.Now;
        }

        public void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            serviceInvokeInfo.ConcurrentRequests--;
            if (elapsedTotalMilliseconds > 0)
            {
                serviceInvokeInfo.AET = serviceInvokeInfo.AET.HasValue
                    ? (serviceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }

        public void ExecFail((string, IAddressModel) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ServiceInvokeInfo());
            serviceInvokeInfo.ConcurrentRequests--;
            serviceInvokeInfo.FaultRequests++;
            serviceInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }

        public ServiceInstanceInvokeInfo GetServiceInstanceInvokeInfo()
        {
            ServiceInstanceInvokeInfo serviceInstanceInvokeInfo = null;

            if (m_monitor.Count <= 0)
            {
                serviceInstanceInvokeInfo = new ServiceInstanceInvokeInfo();
            }
            else
            {
                serviceInstanceInvokeInfo = new ServiceInstanceInvokeInfo()
                {
                    AET = m_monitor.Values.Sum(p => p.AET) / m_monitor.Count,
                    MaxConcurrentRequests = m_monitor.Values.Max(p => p.ConcurrentRequests),
                    FaultRequests = m_monitor.Values.Sum(p => p.FaultRequests),
                    TotalRequests = m_monitor.Values.Sum(p => p.TotalRequests),
                    FinalInvokeTime = m_monitor.Values.Max(p => p.FinalInvokeTime),
                    FinalFaultInvokeTime = m_monitor.Values.Max(p => p.FinalFaultInvokeTime),
                    FirstInvokeTime = m_monitor.Values.Min(p => p.FirstInvokeTime)
                };
            }


            return serviceInstanceInvokeInfo;
        }

        public IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos()
        {
            var serviceEntryInvokeInfos = new List<ServiceEntryInvokeInfo>();
            foreach (var monitor in m_monitor)
            {
                var serviceEntryInvokeInfo = new ServiceEntryInvokeInfo()
                {
                    ServiceEntryId = monitor.Key.Item1,
                    Address = monitor.Key.Item2.IPEndPoint.ToString(),
                    ServiceInvokeInfo = monitor.Value,
                    IsEnable = _healthCheck.IsHealth(monitor.Key.Item2)
                };
                serviceEntryInvokeInfos.Add(serviceEntryInvokeInfo);
            }

            return serviceEntryInvokeInfos.OrderBy(p=> p.ServiceEntryId).ToArray();
        }
    }
}