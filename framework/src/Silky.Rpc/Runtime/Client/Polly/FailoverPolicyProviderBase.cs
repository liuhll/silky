using Castle.Core.Internal;
using Polly;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Client
{
    public abstract class FailoverPolicyProviderBase : IFailoverPolicyProvider
    {
        protected virtual IRpcEndpoint GetSelectedServerEndpoint()
        {
            var selectedServerEndpoint = RpcContext.Context.GetSelectedServerRpcEndpoint();
            return selectedServerEndpoint;
        }

        public abstract IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters);
        
        public abstract event RpcInvokeFailoverHandle OnInvokeFailover;
        
        public abstract FailoverType FailoverType { get; }
    }
}