using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultInvokeSupervisor : IInvokeSupervisor
    {
        private ConcurrentDictionary<(string, IRpcEndpoint), ClientInvokeInfo> m_monitor = new();
        private readonly IHealthCheck _healthCheck;
       
        public ILogger<DefaultInvokeSupervisor> Logger { get; set; }


        public DefaultInvokeSupervisor(IHealthCheck healthCheck,
            IServiceEntryLocator serviceEntryLocator)
        {
            _healthCheck = healthCheck;
            
            _healthCheck.OnHealthChange += async (model, health) =>
            {
                if (!health)
                {
                    var keys = m_monitor.Keys.Where(p => p.Item2.Equals(model));
                    foreach (var key in keys)
                    {
                        var serviceEntry = serviceEntryLocator.GetServiceEntryById(key.Item1);
                        key.Item2.MakeFusing(serviceEntry.GovernanceOptions.AddressFuseSleepDurationSeconds);
                    }
                }
            };
            _healthCheck.OnRemoveRpcEndpoint += async model =>
            {
                var keys = m_monitor.Keys.Where(p => p.Item2.Equals(model));
                foreach (var key in keys)
                {
                    m_monitor.TryRemove(key, out var _);
                }
            };

            Logger = NullLogger<DefaultInvokeSupervisor>.Instance;
        }

        public void Monitor((string, IRpcEndpoint) item)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ClientInvokeInfo());
            serviceInvokeInfo.ConcurrentInvokeCount++;
            serviceInvokeInfo.TotalInvokeCount++;
            serviceInvokeInfo.FinalInvokeTime = DateTime.Now;
        }

        public void ExecSuccess((string, IRpcEndpoint) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ClientInvokeInfo());
            serviceInvokeInfo.ConcurrentInvokeCount--;
            if (elapsedTotalMilliseconds > 0)
            {
                serviceInvokeInfo.AET = serviceInvokeInfo.AET.HasValue
                    ? (serviceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }

        public void ExecFail((string, IRpcEndpoint) item, double elapsedTotalMilliseconds)
        {
            var serviceInvokeInfo = m_monitor.GetOrAdd(item, new ClientInvokeInfo());
            serviceInvokeInfo.ConcurrentInvokeCount--;
            serviceInvokeInfo.FaultInvokeCount++;
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
                    MaxConcurrentRequests = m_monitor.Values.Max(p => p.ConcurrentInvokeCount),
                    FaultRequests = m_monitor.Values.Sum(p => p.FaultInvokeCount),
                    TotalRequests = m_monitor.Values.Sum(p => p.TotalInvokeCount),
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
                    ClientInvokeInfo = monitor.Value,
                    IsEnable = _healthCheck.IsHealth(monitor.Key.Item2)
                };
                serviceEntryInvokeInfos.Add(serviceEntryInvokeInfo);
            }

            return serviceEntryInvokeInfos.OrderBy(p => p.ServiceEntryId).ToArray();
        }
    }
}