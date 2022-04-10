using Polly;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public abstract class InvokeFailoverPolicyProviderBase : IInvokeFailoverPolicyProvider
    {
        protected virtual IRpcEndpoint GetSelectedServerEndpoint()
        {
            var selectedHost = RpcContext.Context.GetSelectedServerHost();
            if (selectedHost == null)
            {
                return null;
            }

            var selectedServerPort = RpcContext.Context.GetSelectedServerPort();
            var selectedServerServiceProtocol = RpcContext.Context.GetSelectedServerServiceProtocol();
            var selectedServerEndpoint =
                RpcEndpointHelper.CreateRpcEndpoint(selectedHost, selectedServerPort, selectedServerServiceProtocol);
            return selectedServerEndpoint;
        }
        
        
        public abstract IAsyncPolicy<object> Create(string serviceEntryId);

        public abstract event RpcInvokeFailoverHandle OnInvokeFailover;

        public abstract FailoverType FailoverType { get; }
    }
}