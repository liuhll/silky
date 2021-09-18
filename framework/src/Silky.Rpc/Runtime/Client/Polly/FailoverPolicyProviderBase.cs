using Polly;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public abstract class FailoverPolicyProviderBase : IFailoverPolicyProvider
    {
        protected virtual IRpcEndpoint GetSelectedServerEndpoint()
        {
            var selectedAddress = RpcContext.Context.GetSelectedServerAddress();
            if (selectedAddress == null)
            {
                return null;
            }

            var selectedServerPort = RpcContext.Context.GetSelectedServerPort();
            var selectedServerServiceProtocol = RpcContext.Context.GetSelectedServerServiceProtocol();
            var selectedServerEndpoint =
                AddressHelper.CreateRpcEndpoint(selectedAddress, selectedServerPort, selectedServerServiceProtocol);
            return selectedServerEndpoint;
        }

        public abstract IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters);

        public abstract event RpcInvokeFailoverHandle OnInvokeFailover;

        public abstract FailoverType FailoverType { get; }
    }
}