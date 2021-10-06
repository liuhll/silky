using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silky.Caching;
using Silky.Core.Rpc;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Monitor.Handle
{
    public class DefaultServerHandleMonitor : IServerHandleMonitor
    {
        private readonly IDistributedCache<ServerHandleInfo> _distributedCache;

        public DefaultServerHandleMonitor(IDistributedCache<ServerHandleInfo> distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public ServerHandleInfo Monitor((string, string) item)
        {
            var serverHandleInfo = _distributedCache.Get(GetCacheKey(item));
            if (serverHandleInfo == null)
            {
                serverHandleInfo = new ServerHandleInfo();
            }

            serverHandleInfo.ServiceEntryId = item.Item1;
            serverHandleInfo.Address = item.Item2;
            serverHandleInfo.ConcurrentHandleCount++;
            serverHandleInfo.TotalHandleCount++;
            serverHandleInfo.FinalHandleTime = DateTime.Now;
            return serverHandleInfo;
        }

        public void ExecSuccess((string, string) item, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo)
        {
            serverHandleInfo.ConcurrentHandleCount--;
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
            serverHandleInfo.ConcurrentHandleCount--;
            serverHandleInfo.FaultHandleCount++;
            serverHandleInfo.FinalHandleTime = DateTime.Now;
            if (isSeriousError)
            {
                serverHandleInfo.SeriousError++;
                serverHandleInfo.SeriousErrorTime = DateTime.Now;
            }

            serverHandleInfo.ConcurrentHandleCount--;
            if (elapsedTotalMilliseconds > 0)
            {
                serverHandleInfo.AET = serverHandleInfo.AET.HasValue
                    ? (serverHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _distributedCache.Set(GetCacheKey(item), serverHandleInfo);
        }

        public async Task<ServerInstanceHandleInfo> GetServiceInstanceHandleInfo()
        {
            ServerInstanceHandleInfo serverInstanceHandleInfo = null;

            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:ServerHandleSupervisor:{RpcContext.Context.Connection.LocalAddress}:*");

            if (cacheKeys.Count <= 0)
            {
                serverInstanceHandleInfo = new ServerInstanceHandleInfo();
            }
            else
            {
                var serverInstanceHandleInfos =
                    (await _distributedCache.GetManyAsync(cacheKeys)).Select(p => p.Value).ToArray();
                serverInstanceHandleInfo = new ServerInstanceHandleInfo()
                {
                    AET = serverInstanceHandleInfos.Sum(p => p.AET) / serverInstanceHandleInfos.Length,
                    FaultHandleCount = serverInstanceHandleInfos.Sum(p => p.FaultHandleCount),
                    TotalHandleCount = serverInstanceHandleInfos.Sum(p => p.TotalHandleCount),
                    FirstHandleTime = serverInstanceHandleInfos.Min(p => p.FirstHandleTime),
                    FinalHandleTime = serverInstanceHandleInfos.Max(p => p.FinalHandleTime),
                    FinalFaultHandleTime = serverInstanceHandleInfos.Min(p => p.FinalFaultHandleTime),
                    TotalSeriousErrorCount = serverInstanceHandleInfos.Sum(p => p.SeriousError),
                    FinalSeriousErrorTime = serverInstanceHandleInfos.Max(p => p.SeriousErrorTime),
                };
            }


            return serverInstanceHandleInfo;
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
    }
}