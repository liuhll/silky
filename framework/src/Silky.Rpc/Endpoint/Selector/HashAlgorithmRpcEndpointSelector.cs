using System.Collections.Concurrent;
using System.Linq;
using Silky.Core;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Endpoint.Selector
{
    public class HashAlgorithmRpcEndpointSelector : RpcEndpointSelectorBase
    {
        private ConcurrentDictionary<string, ConsistentHash<ISilkyEndpoint>> _consistentHashAddressPools = new();

        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        private readonly IServerManager _serverManager;

        public HashAlgorithmRpcEndpointSelector(IRpcEndpointMonitor rpcEndpointMonitor, IServerManager serverManager)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _serverManager = serverManager;
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
            _serverManager.OnUpdateRpcEndpoint += async (hostName, endpoints) =>
            {
                foreach (var endpoint in endpoints)
                {
                    var updateItems = _consistentHashAddressPools
                        .Where(p => p.Key.Contains(hostName))
                        .Select(p => p.Value);
                    foreach (var consistentHash in updateItems)
                    {
                        if (!consistentHash.ContainNode(endpoint))
                        {
                            consistentHash.Add(endpoint);
                        }
                    }
                }
            };
        }

        public override ShuntStrategy ShuntStrategy { get; } = ShuntStrategy.HashAlgorithm;

        protected override ISilkyEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context)
        {
            Check.NotNullOrEmpty(context.Hash, nameof(context.Hash));
            var addressModels = _consistentHashAddressPools.GetOrAdd(context.MonitorId, v =>
            {
                var consistentHash = new ConsistentHash<ISilkyEndpoint>();
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