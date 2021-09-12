using Silky.Core.Extensions;

namespace Silky.Core.Rpc
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

        public static string GetServerKey(this RpcContext rpcContext)
        {
            var serviceKey = rpcContext.GetAttachment(AttachmentKeys.ServiceKey);
            return serviceKey?.ToString();
        }
        
        public static string GetMessageId(this RpcContext rpcContext)
        {
            var messageId = rpcContext.GetAttachment(AttachmentKeys.MessageId);
            return messageId?.ToString();
        }
    }
}