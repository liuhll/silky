using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Caching;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Monitor.Invoke
{
    public class DefaultInvokeMonitor : IInvokeMonitor
    {
        private readonly IDistributedCache<ClientInvokeInfo> _distributedCache;
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        public ILogger<DefaultInvokeMonitor> Logger { get; set; }

        public DefaultInvokeMonitor(IDistributedCache<ClientInvokeInfo> distributedCache,
            IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _distributedCache = distributedCache;
            _rpcEndpointMonitor = rpcEndpointMonitor;
            Logger = NullLogger<DefaultInvokeMonitor>.Instance;
        }

        public ClientInvokeInfo Monitor((string, IRpcEndpoint) item)
        {
            var cacheKey = GetCacheKey(item);
            var clientInvokeInfo = _distributedCache.Get(cacheKey);
            if (clientInvokeInfo == null)
            {
                clientInvokeInfo = new ClientInvokeInfo();
            }

            clientInvokeInfo.Address = item.Item2.GetAddress();
            clientInvokeInfo.ServiceEntryId = item.Item1;
            clientInvokeInfo.ConcurrentInvokeCount++;
            clientInvokeInfo.TotalInvokeCount++;
            clientInvokeInfo.FinalInvokeTime = DateTime.Now;
            return clientInvokeInfo;
        }

        public void ExecSuccess((string, IRpcEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo)
        {
            clientInvokeInfo.ConcurrentInvokeCount--;
            if (elapsedTotalMilliseconds > 0)
            {
                clientInvokeInfo.AET = clientInvokeInfo.AET.HasValue
                    ? (clientInvokeInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _distributedCache.Set(GetCacheKey(item), clientInvokeInfo);
        }

        public void ExecFail((string, IRpcEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo)
        {
            clientInvokeInfo.ConcurrentInvokeCount--;
            clientInvokeInfo.FaultInvokeCount++;
            clientInvokeInfo.FinalFaultInvokeTime = DateTime.Now;
            _distributedCache.Set(GetCacheKey(item), clientInvokeInfo);
        }

        public async Task<ServiceInstanceInvokeInfo> GetServiceInstanceInvokeInfo()
        {
            ServiceInstanceInvokeInfo serviceInstanceInvokeInfo = null;

            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:*");
            if (cacheKeys.Count <= 0)
            {
                serviceInstanceInvokeInfo = new ServiceInstanceInvokeInfo();
            }
            else
            {
                var clientInvokeInfos =
                    (await _distributedCache.GetManyAsync(cacheKeys)).Select(p => p.Value).ToArray();
                serviceInstanceInvokeInfo = new ServiceInstanceInvokeInfo()
                {
                    AET = clientInvokeInfos.Sum(p => p.AET) / clientInvokeInfos.Length,
                    FaultInvokeCount = clientInvokeInfos.Sum(p => p.FaultInvokeCount),
                    TotalInvokeCount = clientInvokeInfos.Sum(p => p.TotalInvokeCount),
                    FinalInvokeTime = clientInvokeInfos.Max(p => p.FinalInvokeTime),
                    FinalFaultInvokeTime = clientInvokeInfos.Max(p => p.FinalFaultInvokeTime),
                    FirstInvokeTime = clientInvokeInfos.Min(p => p.FirstInvokeTime)
                };
            }


            return serviceInstanceInvokeInfo;
        }

        public async Task<IReadOnlyCollection<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos()
        {
            var serviceEntryInvokeInfos = new List<ServiceEntryInvokeInfo>();
            var cacheKeys =
                await _distributedCache.SearchKeys(
                    $"*:InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:*");
            if (cacheKeys.Count <= 0)
            {
                return serviceEntryInvokeInfos;
            }

            var clientInvokeInfos =
                (await _distributedCache.GetManyAsync(cacheKeys)).Select(p => p.Value).ToArray();
            foreach (var clientInvokeInfo in clientInvokeInfos)
            {
                var serviceEntryInvokeInfo = new ServiceEntryInvokeInfo()
                {
                    ServiceEntryId = clientInvokeInfo.ServiceEntryId,
                    Address = clientInvokeInfo.Address,
                    ClientInvokeInfo = clientInvokeInfo,
                    IsEnable = _rpcEndpointMonitor.IsEnable(
                        RpcEndpointHelper.CreateRpcEndpoint(clientInvokeInfo.Address, ServiceProtocol.Tcp))
                };
                serviceEntryInvokeInfos.Add(serviceEntryInvokeInfo);
            }

            return serviceEntryInvokeInfos.OrderBy(p => p.ServiceEntryId).ToArray();
        }

        private string GetCacheKey((string, IRpcEndpoint) item)
        {
            var cacheKey =
                $"InvokeSupervisor:{RpcContext.Context.Connection.LocalAddress}:{item.Item1}:{item.Item2.GetAddress()}";
            return cacheKey;
        }
    }
}