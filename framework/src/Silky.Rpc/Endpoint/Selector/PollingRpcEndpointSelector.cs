using System.Collections.Concurrent;
using System.Linq;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Endpoint.Selector
{
    public class PollingRpcEndpointSelector : RpcEndpointSelectorBase
    {
        private ConcurrentDictionary<string, (int, ISilkyEndpoint[])> addressesPools = new();


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
            _serverManager.OnRemoveRpcEndpoint += async (hostName, endpoint) =>
            {
                var removeKeys = addressesPools.Where(p =>
                        p.Value.Item2.Any(r => r.Host.Equals(hostName)) ||
                        p.Value.Item2.Any(q => Equals(endpoint)))
                    .Select(p => p.Key);
                foreach (var removeKey in removeKeys)
                {
                    addressesPools.TryRemove(removeKey, out _);
                }
            };
        }

        public override ShuntStrategy ShuntStrategy { get; } = ShuntStrategy.Polling;

        protected override ISilkyEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context)
        {
            var index = 0;

            if (addressesPools.TryGetValue(context.MonitorId, out var selectAddressItem))
            {
                if (selectAddressItem.Item2.Length != context.AddressModels.Length)
                {
                    selectAddressItem = (0, context.AddressModels);
                    addressesPools.AddOrUpdate(context.MonitorId, selectAddressItem, (k, v) => selectAddressItem);
                }
                else
                {
                    index = selectAddressItem.Item1 >= selectAddressItem.Item2.Length ? 0 : selectAddressItem.Item1;
                }
            }
            else
            {
                selectAddressItem = (0, context.AddressModels);
                addressesPools.AddOrUpdate(context.MonitorId, selectAddressItem, (k, v) => selectAddressItem);
            }

            var selectAddress = selectAddressItem.Item2[index];
            selectAddressItem.Item1 = index + 1;
            addressesPools.AddOrUpdate(context.MonitorId, selectAddressItem, (k, v) => selectAddressItem);
            return selectAddress;
        }
    }
}