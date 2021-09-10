using Silky.Core.Extensions;
using Silky.Core.Rpc;

namespace Silky.Rpc.Transport
{
    public static class RpcContextExtensions
    {
        public static bool IsGateway(this RpcContext rpcContext)
        {
            var isGateway = rpcContext.GetAttachment(AttachmentKeys.IsGatewayHost);
            if (isGateway == null)
            {
                return false;
            }

            return isGateway.To<bool>();
        }
        
        public static string GetClientAddress(this RpcContext rpcContext)
        {
            var clientAddress = rpcContext.GetAttachment(AttachmentKeys.ClientAddress);
            return clientAddress?.ToString();
        }
        public static string GetServerAddress(this RpcContext rpcContext)
        {
            var serverAddress = rpcContext.GetAttachment(AttachmentKeys.ServerAddress);
            return serverAddress?.ToString();
        }
    }
}