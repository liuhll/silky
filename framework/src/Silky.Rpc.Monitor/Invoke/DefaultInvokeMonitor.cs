using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Monitor.Provider;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Monitor.Invoke
{
    public class DefaultInvokeMonitor : IInvokeMonitor
    {
        private readonly IMonitorProvider _monitorProvider;
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public ILogger<DefaultInvokeMonitor> Logger { get; set; }

        public DefaultInvokeMonitor(
            IMonitorProvider monitorProvider,
            IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _monitorProvider = monitorProvider;
            _rpcEndpointMonitor = rpcEndpointMonitor;
            Logger = NullLogger<DefaultInvokeMonitor>.Instance;
        }

        public ClientInvokeInfo Monitor((string, ISilkyEndpoint) item)
        {
            if (item.Item2 == null)
            {
                return null;
            }

            lock (_monitorProvider.InstanceInvokeInfo)
            {
                _monitorProvider.InstanceInvokeInfo.ConcurrentCount++;
                if (_monitorProvider.InstanceInvokeInfo.ConcurrentCount >
                    _monitorProvider.InstanceInvokeInfo.MaxConcurrentCount)
                {
                    _monitorProvider.InstanceInvokeInfo.MaxConcurrentCount =
                        _monitorProvider.InstanceInvokeInfo.ConcurrentCount;
                }

                _monitorProvider.InstanceInvokeInfo.FirstInvokeTime ??= DateTime.Now;
                _monitorProvider.InstanceInvokeInfo.FinalInvokeTime = DateTime.Now;
                _monitorProvider.InstanceInvokeInfo.TotalInvokeCount += 1;
            }

            var cacheKey = GetCacheKey(item);
            var clientInvokeInfo = _monitorProvider.GetInvokeInfo(cacheKey);
            clientInvokeInfo.IsEnable = true;
            clientInvokeInfo.Address = item.Item2.GetAddress();
            clientInvokeInfo.ServiceEntryId = item.Item1;
            clientInvokeInfo.TotalInvokeCount++;
            clientInvokeInfo.FinalInvokeTime = DateTime.Now;
            return clientInvokeInfo;
        }


        public void ExecSuccess((string, ISilkyEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo)
        {
            if (item.Item2 == null || clientInvokeInfo == null)
            {
                return;
            }

            lock (_monitorProvider.InstanceInvokeInfo)
            {
                _monitorProvider.InstanceInvokeInfo.ConcurrentCount--;
                if (elapsedTotalMilliseconds > 0)
                {
                    _monitorProvider.InstanceInvokeInfo.AET = _monitorProvider.InstanceInvokeInfo.AET.HasValue
                        ? (_monitorProvider.InstanceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            if (elapsedTotalMilliseconds > 0)
            {
                clientInvokeInfo.AET = clientInvokeInfo.AET.HasValue
                    ? (clientInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _monitorProvider.SetClientInvokeInfo(GetCacheKey(item), clientInvokeInfo);
        }

        public void ExecFail((string, ISilkyEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo)
        {
            if (item.Item2 == null || clientInvokeInfo == null)
            {
                return;
            }

            lock (_monitorProvider.InstanceInvokeInfo)
            {
                _monitorProvider.InstanceInvokeInfo.ConcurrentCount--;
                _monitorProvider.InstanceInvokeInfo.FaultInvokeCount++;
                _monitorProvider.InstanceInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
                if (elapsedTotalMilliseconds > 0)
                {
                    _monitorProvider.InstanceInvokeInfo.AET = _monitorProvider.InstanceInvokeInfo.AET.HasValue
                        ? (_monitorProvider.InstanceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            clientInvokeInfo.IsEnable = _rpcEndpointMonitor.IsEnable(item.Item2);
            clientInvokeInfo.FaultInvokeCount++;
            clientInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
            if (elapsedTotalMilliseconds > 0)
            {
                clientInvokeInfo.AET = clientInvokeInfo.AET.HasValue
                    ? (clientInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _monitorProvider.SetClientInvokeInfo(GetCacheKey(item), clientInvokeInfo);
        }

        public Task<ServerInstanceInvokeInfo> GetServerInstanceInvokeInfo()
        {
            lock (_monitorProvider.InstanceInvokeInfo)
            {
                return Task.FromResult(_monitorProvider.InstanceInvokeInfo);
            }
        }

        public Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos()
        {
            return _monitorProvider.GetServiceEntryInvokeInfos();
        }

        private string GetCacheKey((string, ISilkyEndpoint) item)
        {
            var cacheKey =
                $"InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:{item.Item1}:{item.Item2.GetAddress()}";
            return cacheKey;
        }
    }
}