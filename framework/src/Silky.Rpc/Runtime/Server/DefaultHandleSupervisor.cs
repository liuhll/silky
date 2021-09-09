using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultHandleSupervisor : IHandleSupervisor
    {
        private ConcurrentDictionary<(string, string), ServiceHandleInfo> m_monitor = new();

        public void Monitor((string, string) item)
        {
            var serviceHandleInfo = m_monitor.GetOrAdd(item, new ServiceHandleInfo());
            serviceHandleInfo.ConcurrentHandles++;
            serviceHandleInfo.TotalHandles++;
            serviceHandleInfo.FinalHandleTime = DateTime.Now;
        }

        public void ExecSuccess((string, string) item, double elapsedTotalMilliseconds)
        {
            var serviceHandleInfo = m_monitor.GetOrAdd(item, new ServiceHandleInfo());
            serviceHandleInfo.ConcurrentHandles--;
            if (elapsedTotalMilliseconds > 0)
            {
                serviceHandleInfo.AET = serviceHandleInfo.AET.HasValue
                    ? (serviceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            m_monitor.AddOrUpdate(item, serviceHandleInfo, (key, _) => serviceHandleInfo);
        }

        public void ExecFail((string, string) item, bool isSeriousError, double elapsedTotalMilliseconds)
        {
            var serviceHandleInfo = m_monitor.GetOrAdd(item, new ServiceHandleInfo());
            serviceHandleInfo.ConcurrentHandles--;
            serviceHandleInfo.FaultHandles++;
            serviceHandleInfo.FinalHandleTime = DateTime.Now;
            if (isSeriousError)
            {
                serviceHandleInfo.SeriousError++;
                serviceHandleInfo.SeriousErrorTime = DateTime.Now;
            }
            serviceHandleInfo.ConcurrentHandles--;
            if (elapsedTotalMilliseconds > 0)
            {
                serviceHandleInfo.AET = serviceHandleInfo.AET.HasValue
                    ? (serviceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            m_monitor.AddOrUpdate(item, serviceHandleInfo, (key, _) => serviceHandleInfo);
        }

        public ServiceInstanceHandleInfo GetServiceInstanceHandleInfo()
        {
            ServiceInstanceHandleInfo serviceInstanceHandleInfo = null;
            if (m_monitor.Count <= 0)
            {
                serviceInstanceHandleInfo = new ServiceInstanceHandleInfo();
            }
            else
            {
                serviceInstanceHandleInfo = new ServiceInstanceHandleInfo()
                {
                    AET = m_monitor.Values.Sum(p => p.AET) / m_monitor.Count,
                    MaxConcurrentHandles = m_monitor.Values.Max(p => p.ConcurrentHandles),
                    FaultHandles = m_monitor.Values.Sum(p => p.FaultHandles),
                    TotalHandles = m_monitor.Values.Sum(p => p.TotalHandles),
                    FirstHandleTime = m_monitor.Values.Max(p => p.FirstHandleTime),
                    FinalHandleTime = m_monitor.Values.Max(p => p.FinalHandleTime),
                    FinalFaultHandleTime = m_monitor.Values.Min(p => p.FinalFaultHandleTime),
                    TotalSeriousError = m_monitor.Values.Sum(p => p.SeriousError),
                    FinalSeriousErrorTime = m_monitor.Values.Max(p => p.SeriousErrorTime),
                };
            }


            return serviceInstanceHandleInfo;
        }

        public IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos()
        {
            var serviceEntryInvokeInfos = new List<ServiceEntryHandleInfo>();
            foreach (var monitor in m_monitor)
            {
                var serviceEntryInvokeInfo =  new ServiceEntryHandleInfo()
                {
                    ServiceEntryId = monitor.Key.Item1,
                    Address = monitor.Key.Item2,
                    ServiceHandleInfo = monitor.Value
                };
                serviceEntryInvokeInfos.Add(serviceEntryInvokeInfo);
            }

            return serviceEntryInvokeInfos.OrderBy(p=> p.ServiceEntryId).ToArray();
        }
    }
}