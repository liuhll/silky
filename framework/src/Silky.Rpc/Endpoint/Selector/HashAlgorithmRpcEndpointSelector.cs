using System.Collections.Concurrent;
using System.Linq;
using Silky.Core;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.Rpc.Endpoint.Selector
{
    public class HashAlgorithmRpcEndpointSelector : RpcEndpointSelectorBase
    {
        private ConcurrentDictionary<string, ConsistentHash<IRpcEndpoint>> _consistentHashAddressPools = new();

        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public HashAlgorithmRpcEndpointSelector(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += async rpcEndpoint =>
            {
                var removeItems = _consistentHashAddressPools
                    .Where(p => p.Value.ContainNode(rpcEndpoint))
                    .Select(p => p.Value);
                foreach (var consistentHash in removeItems)
                {
                    consistentHash.Remove(rpcEndpoint);
                }
            };

            _rpcEndpointMonitor.OnStatusChange += async (rpcEndpoint, isHealth) =>
            {
                var changeItems = _consistentHashAddressPools
                    .Where(p => p.Value.ContainNode(rpcEndpoint))
                    .Select(p => p.Value);
                foreach (var consistentHash in changeItems)
                {
                    if (!isHealth)
                    {
                        consistentHash.Remove(rpcEndpoint);
                    }
                    else
                    {
                        consistentHash.Add(rpcEndpoint);
                    }
                }
            };
        }

        public override ShuntStrategy ShuntStrategy { get; } = ShuntStrategy.HashAlgorithm;

        protected override IRpcEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context)
        {
            Check.NotNullOrEmpty(context.Hash, nameof(context.Hash));
            var addressModels = _consistentHashAddressPools.GetOrAdd(context.MonitorId, v =>
            {
                var consistentHash = new ConsistentHash<IRpcEndpoint>();
                foreach (var address in context.AddressModels)
                {
                    consistentHash.Add(address);
                }

                return consistentHash;
            });
            if (addressModels.GetNodeCount() < context.AddressModels.Length)
            {
                foreach (var addressModel in context.AddressModels)
                {
                    if (!addressModels.ContainNode(addressModel))
                    {
                        addressModels.Add(addressModel);
                    }
                }
            }

            return addressModels.GetItemNode(context.Hash);
        }
    }
}