using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Monitor.Handle
{
    public class DefaultServerHandleMonitor : IServerHandleMonitor
    {
        private readonly IDistributedCache<ServerHandleInfo> _distributedCache;
        private ServerInstanceHandleInfo _serverInstanceHandleInfo = new();


        public DefaultServerHandleMonitor(IDistributedCache<ServerHandleInfo> distributedCache,IOptionsMonitor<GovernanceOptions> governanceOptions)
        {
            _distributedCache = distributedCache;
            _serverInstanceHandleInfo.AllowMaxConcurrentCount = governanceOptions.CurrentValue.MaxConcurrentHandlingCount;
            governanceOptions.OnChange(options =>
            {
                _serverInstanceHandleInfo.AllowMaxConcurrentCount = options.MaxConcurrentHandlingCount;
            });
           
        }

        public ServerHandleInfo Monitor((string, string) item)
        {
            lock (_serverInstanceHandleInfo)
            {
                _serverInstanceHandleInfo.ConcurrentCount++;
                if (_serverInstanceHandleInfo.ConcurrentCount > _serverInstanceHandleInfo.MaxConcurrentCount)
                {
                    _serverInstanceHandleInfo.MaxConcurrentCount = _serverInstanceHandleInfo.ConcurrentCount;
                }

                _serverInstanceHandleInfo.FirstHandleTime ??= DateTime.Now;
                _serverInstanceHandleInfo.FinalHandleTime = DateTime.Now;
                _serverInstanceHandleInfo.TotalHandleCount += 1;
            }

            var serverHandleInfo = _distributedCache.Get(GetCacheKey(item)) ?? new ServerHandleInfo();
            serverHandleInfo.ServiceEntryId = item.Item1;
            serverHandleInfo.Address = item.Item2;
            serverHandleInfo.TotalHandleCount++;
            serverHandleInfo.FinalHandleTime = DateTime.Now;
            return serverHandleInfo;
        }


        public void ExecSuccess((string, string) item, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo)
        {
            lock (_serverInstanceHandleInfo)
            {
                _serverInstanceHandleInfo.ConcurrentCount--;
                if (elapsedTotalMilliseconds > 0)
                {
                    _serverInstanceHandleInfo.AET = _serverInstanceHandleInfo.AET.HasValue
                        ? (_serverInstanceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            if (elapsedTotalMilliseconds > 0)
            {
                serverHandleInfo.AET = serverHandleInfo.AET.HasValue
                    ? (serverHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _distributedCache.Set(GetCacheKey(item), serverHandleInfo);
        }

        public void ExecFail((string, string) item, bool isSeriousError, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo)
        {
            lock (_serverInstanceHandleInfo)
            {
                _serverInstanceHandleInfo.ConcurrentCount--;
                _serverInstanceHandleInfo.FaultHandleCount++;
                _serverInstanceHandleInfo.FinalFaultHandleTime = DateTime.Now;

                if (isSeriousError)
                {
                    _serverInstanceHandleInfo.TotalSeriousErrorCount++;
                    _serverInstanceHandleInfo.FinalSeriousErrorTime = DateTime.Now;
                }

                if (elapsedTotalMilliseconds > 0)
                {
                    _serverInstanceHandleInfo.AET = _serverInstanceHandleInfo.AET.HasValue
                        ? (_serverInstanceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            serverHandleInfo.FaultHandleCount++;
            serverHandleInfo.FinalSeriousErrorTime = DateTime.Now;
            if (isSeriousError)
            {
                serverHandleInfo.SeriousErrorCount++;
                serverHandleInfo.SeriousErrorTime = DateTime.Now;
            }

            if (elapsedTotalMilliseconds > 0)
            {
                serverHandleInfo.AET = serverHandleInfo.AET.HasValue
                    ? (serverHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _distributedCache.Set(GetCacheKey(item), serverHandleInfo);
        }

        public Task<ServerInstanceHandleInfo> GetServerInstanceHandleInfo()
        {
            lock (_serverInstanceHandleInfo)
            {
                return Task.FromResult(_serverInstanceHandleInfo);
            }
        }

        public async Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos()
        {
            var serviceEntryHandleInfos = new List<ServerHandleInfo>();
            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:ServerHandleSupervisor:{RpcContext.Context.Connection.LocalAddress}:*");

            if (cacheKeys.Count <= 0)
            {
                return serviceEntryHandleInfos;
            }

            var serverHandleInfos =
                (await _distributedCache.GetManyAsync(cacheKeys)).Select(p => p.Value).ToArray();
            foreach (var serverHandleInfo in serverHandleInfos)
            {
                serviceEntryHandleInfos.Add(serverHandleInfo);
            }

            return serviceEntryHandleInfos.OrderBy(p => p.ServiceEntryId).ToArray();
        }

        private string GetCacheKey((string, string) item)
        {
            var cacheKey =
                $"ServerHandleSupervisor:{RpcContext.Context.Connection.LocalAddress}:{item.Item1}:{item.Item2}";
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
                    $"*:ServerHandleSupervisor:{address}:*");
            foreach (var cacheKey in cacheKeys)
            {
                await _distributedCache.RemoveAsync(cacheKey);
            }
        }
    }
}