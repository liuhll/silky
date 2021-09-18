using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class RpcContextExtensions
    {
        public static void SetRcpInvokeAddressInfo(this RpcContext rpcContext,AddressDescriptor serverAddress,AddressDescriptor clientAddress)
        {
            rpcContext
                .SetAttachment(AttachmentKeys.ServerAddress, serverAddress.Address);
            rpcContext
                .SetAttachment(AttachmentKeys.ServerServiceProtocol, serverAddress.ServiceProtocol.ToString());
            rpcContext.SetAttachment(AttachmentKeys.ClientAddress,
                clientAddress.Address);
            rpcContext.SetAttachment(AttachmentKeys.ClientServiceProtocol,
                clientAddress.ServiceProtocol.ToString());
        }
        
        public static ServiceProtocol GetServerServiceProtocol(this RpcContext rpcContext)
        {
            var serverServiceProtocol = rpcContext.GetAttachment(AttachmentKeys.ServerServiceProtocol);
            return serverServiceProtocol.To<ServiceProtocol>();
        }
    }
}