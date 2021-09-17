using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class RpcContextExtensions
    {
        public static void SetRcpInvokeAddressInfo(this RpcContext rpcContext,IAddressModel serverAddress,IAddressModel clientAddress)
        {
            rpcContext
                .SetAttachment(AttachmentKeys.ServerAddress, serverAddress.IPEndPoint.ToString());
            rpcContext
                .SetAttachment(AttachmentKeys.ServerServiceProtocol, serverAddress.ServiceProtocol.ToString());
            rpcContext.SetAttachment(AttachmentKeys.ClientAddress,
                clientAddress.IPEndPoint.ToString());
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