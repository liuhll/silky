using System;
using System.Collections.Concurrent;

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
            serviceHandleInfo.AET = serviceHandleInfo.AET.HasValue
                ? (serviceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                : elapsedTotalMilliseconds;

            m_monitor.AddOrUpdate(item, serviceHandleInfo, (key, _) => serviceHandleInfo);
        }

        public void ExecFail((string, string) item, bool isBusinessException, double elapsedTotalMilliseconds)
        {
            var serviceHandleInfo = m_monitor.GetOrAdd(item, new ServiceHandleInfo());
            serviceHandleInfo.ConcurrentHandles--;
            serviceHandleInfo.FaultHandles++;
            serviceHandleInfo.FinalHandleTime = DateTime.Now;
            if (!isBusinessException)
            {
                serviceHandleInfo.SeriousError++;
                serviceHandleInfo.SeriousErrorTime = DateTime.Now;
            }
           
            m_monitor.AddOrUpdate(item, serviceHandleInfo, (key, _) => serviceHandleInfo);
        }
    }
}