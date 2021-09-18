using Castle.Core.Internal;
using Polly;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Client
{
    public abstract class FailoverPolicyProviderBase : IFailoverPolicyProvider
    {
        protected virtual IRpcAddress GetSelectedServerAddress()
        {
            var serverAddress = RpcContext.Context.GetServerAddress();
           
            if (serverAddress.IsNullOrEmpty())
            {
                return null;
            }
            var serverServiceProtocol = RpcContext.Context.GetServerServiceProtocol();
            var serviceAddressModel =
                AddressHelper.CreateAddressModel(serverAddress, serverServiceProtocol);
            return serviceAddressModel;
        }

        public abstract IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters);
        
        public abstract event RpcInvokeFailoverHandle OnInvokeFailover;
        
        public abstract FailoverType FailoverType { get; }
    }
}