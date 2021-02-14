using System;
using System.Collections.Concurrent;
using Lms.Core.Exceptions;
using Lms.Rpc.Address;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceSupervisor : IRemoteServiceSupervisor
    {
        private ConcurrentDictionary<(string, IAddressModel), ServiceInvokeInfo> m_monitor = new();
        public ILogger<DefaultRemoteServiceSupervisor> Logger { get; set; }

        public DefaultRemoteServiceSupervisor()
        {
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
            serviceInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
            m_monitor.AddOrUpdate(item, serviceInvokeInfo, (key, _) => serviceInvokeInfo);
        }
    }
}