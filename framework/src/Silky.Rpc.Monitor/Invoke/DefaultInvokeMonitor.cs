using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Monitor.Invoke
{
    public class DefaultInvokeMonitor : IInvokeMonitor
    {
        private readonly IDistributedCache<ClientInvokeInfo> _distributedCache;
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        private ServerInstanceInvokeInfo _serverInstanceInvokeInfo = new();
        public ILogger<DefaultInvokeMonitor> Logger { get; set; }

        public DefaultInvokeMonitor(IDistributedCache<ClientInvokeInfo> distributedCache,
            IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _distributedCache = distributedCache;
            _rpcEndpointMonitor = rpcEndpointMonitor;
            Logger = NullLogger<DefaultInvokeMonitor>.Instance;
        }

        public ClientInvokeInfo Monitor((string, ISilkyEndpoint) item)
        {
            lock (_serverInstanceInvokeInfo)
            {
                _serverInstanceInvokeInfo.ConcurrentCount++;
                if (_serverInstanceInvokeInfo.ConcurrentCount > _serverInstanceInvokeInfo.MaxConcurrentCount)
                {
                    _serverInstanceInvokeInfo.MaxConcurrentCount = _serverInstanceInvokeInfo.ConcurrentCount;
                }

                _serverInstanceInvokeInfo.FirstInvokeTime ??= DateTime.Now;
                _serverInstanceInvokeInfo.FinalInvokeTime = DateTime.Now;
                _serverInstanceInvokeInfo.TotalInvokeCount += 1;
            }

            var cacheKey = GetCacheKey(item);
            var clientInvokeInfo = _distributedCache.Get(cacheKey) ?? new ClientInvokeInfo();
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
            lock (_serverInstanceInvokeInfo)
            {
                _serverInstanceInvokeInfo.ConcurrentCount--;
                if (elapsedTotalMilliseconds > 0)
                {
                    _serverInstanceInvokeInfo.AET = _serverInstanceInvokeInfo.AET.HasValue
                        ? (_serverInstanceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            if (elapsedTotalMilliseconds > 0)
            {
                clientInvokeInfo.AET = clientInvokeInfo.AET.HasValue
                    ? (clientInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _distributedCache.Set(GetCacheKey(item), clientInvokeInfo);
        }

        public void ExecFail((string, ISilkyEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo)
        {
            lock (_serverInstanceInvokeInfo)
            {
                _serverInstanceInvokeInfo.ConcurrentCount--;
                _serverInstanceInvokeInfo.FaultInvokeCount++;
                _serverInstanceInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
                if (elapsedTotalMilliseconds > 0)
                {
                    _serverInstanceInvokeInfo.AET = _serverInstanceInvokeInfo.AET.HasValue
                        ? (_serverInstanceInvokeInfo.AET + elapsedTotalMilliseconds) / 2
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

            _distributedCache.Set(GetCacheKey(item), clientInvokeInfo);
        }

        public Task<ServerInstanceInvokeInfo> GetServerInstanceInvokeInfo()
        {
            lock (_serverInstanceInvokeInfo)
            {
                return Task.FromResult(_serverInstanceInvokeInfo);
            }
        }

        public async Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos()
        {
            var clientInvokeInfos = new List<ClientInvokeInfo>();
            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:*");
            if (cacheKeys.Count <= 0)
            {
                return clientInvokeInfos;
            }

            clientInvokeInfos =
                (await _distributedCache.GetManyAsync(cacheKeys)).Select(p => p.Value).OrderBy(p => p.ServiceEntryId)
                .ToList();
            return clientInvokeInfos;
        }

        private string GetCacheKey((string, ISilkyEndpoint) item)
        {
            var cacheKey =
                $"InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:{item.Item1}:{item.Item2.GetAddress()}";
            return cacheKey;
        }

        public async Task ClearCache()
        {
            if (EngineContext.Current.IsContainDotNettyTcpModule())
            {
                var localTcpEndpoint = SilkyEndpointHelper.GetLocalRpcEndpoint();
                await RemoveCache(localTcpEndpoint.GetAddress());
            }

            if (EngineContext.Current.IsContainHttpCoreModule())
            {
                var localWebEndpoint = SilkyEndpointHelper.GetLocalWebEndpoint();
                if (localWebEndpoint != null)
                {
                    await RemoveCache(localWebEndpoint.GetAddress());
                }
            }
        }

        private async Task RemoveCache(string address)
        {
            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:InvokeSupervisor:{address}:*");
            foreach (var cacheKey in cacheKeys)
            {
                await _distributedCache.RemoveAsync(cacheKey);
            }
        }
    }
}