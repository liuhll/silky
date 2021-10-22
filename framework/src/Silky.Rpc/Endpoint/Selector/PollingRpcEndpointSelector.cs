using System.Collections.Concurrent;
using System.Linq;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Endpoint.Selector
{
    public class PollingRpcEndpointSelector : RpcEndpointSelectorBase
    {
        private ConcurrentDictionary<string, (int, IRpcEndpoint[])> addressesPools = new();


        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        private readonly IServerManager _serverManager;

        public PollingRpcEndpointSelector(IRpcEndpointMonitor rpcEndpointMonitor,
            IServerManager serverManager)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _serverManager = serverManager;
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += async rpcEndpoint =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(rpcEndpoint)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };

            _rpcEndpointMonitor.OnDisEnable += async rpcEndpoint =>
            {
                var removeKeys = addressesPools.Where(p => p.Value.Item2.Any(q => q.Equals(rpcEndpoint)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };
            _serverManager.OnUpdateRpcEndpoint += async (hostName, endpoints) =>
            {
                var removeKeys = addressesPools.Where(p =>
                        p.Value.Item2.Any(r => r.Host.Equals(hostName)) ||
                        p.Value.Item2.Any(q => endpoints.Any(r => r.Equals(q))))
                    .Select(p => p.Key);
                
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };
        }

        public override ShuntStrategy ShuntStrategy { get; } = ShuntStrategy.Polling;

        protected override IRpcEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context)
        {
            var selectAdderessItem = (0, context.AddressModels);
            var index = 0;
            if (addressesPools.ContainsKey(context.MonitorId))
            {
                selectAdderessItem = addressesPools.GetOrAdd(context.MonitorId, selectAdderessItem);
                index = selectAdderessItem.Item1 >= selectAdderessItem.Item2.Count(p => p.Enabled)
                    ? 0
                    : selectAdderessItem.Item1;
            }

            var enableAddress = selectAdderessItem.Item2.Where(p => p.Enabled).ToArray();
            var selectAdderess = enableAddress[index];
            selectAdderessItem.Item1 = index + 1;
            addressesPools.AddOrUpdate(context.MonitorId, selectAdderessItem, (k, v) => selectAdderessItem);
            return selectAdderess;
        }
    }
}