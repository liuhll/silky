using Silky.Core.Extensions;

namespace Silky.Core.Rpc
{
    public static class RpcContextExtensions
    {
        public static bool IsGateway(this RpcContext rpcContext)
        {
            var isGateway = rpcContext.GetAttachment(AttachmentKeys.IsGateway);
            if (isGateway == null)
            {
                return false;
            }

            return isGateway.To<bool>();
        }

        public static string GetClientHost(this RpcContext rpcContext)
        {
            var clientHost = rpcContext.GetAttachment(AttachmentKeys.ClientHost);
            return clientHost?.ToString();
        }

        public static int GetClientPort(this RpcContext rpcContext)
        {
            var clientPort = rpcContext.GetAttachment(AttachmentKeys.ClientPort);
            return clientPort.To<int>();
        }

        public static int? GetRpcRequestPort(this RpcContext rpcContext)
        {
            var rpcRequestPort = rpcContext.GetAttachment(AttachmentKeys.RpcRequestPort);
            if (rpcRequestPort != null)
            {
                return rpcRequestPort.To<int>();
            }

            return null;
        }

        public static ServiceProtocol GetClientServiceProtocol(this RpcContext rpcContext)
        {
            var clientPort = rpcContext.GetAttachment(AttachmentKeys.ClientServiceProtocol);
            return clientPort.To<ServiceProtocol>();
        }


        public static string GetLocalHost(this RpcContext rpcContext)
        {
            var localAddress = rpcContext.GetAttachment(AttachmentKeys.LocalAddress);
            return localAddress?.ToString();
        }

        public static int GetLocalPort(this RpcContext rpcContext)
        {
            var localPort = rpcContext.GetAttachment(AttachmentKeys.LocalPort);
            return localPort.To<int>();
        }

        public static ServiceProtocol GetLocalServiceProtocol(this RpcContext rpcContext)
        {
            var localServiceProtocol = rpcContext.GetAttachment(AttachmentKeys.LocalServiceProtocol);
            return localServiceProtocol.To<ServiceProtocol>();
        }


        public static string GetServiceKey(this RpcContext rpcContext)
        {
            var serviceKey = rpcContext.GetAttachment(AttachmentKeys.ServiceKey);
            return serviceKey?.ToString();
        }


        public static void SetServiceKey(this RpcContext rpcContext, string serviceKey)
        {
            rpcContext.SetAttachment(AttachmentKeys.ServiceKey, serviceKey);
        }

        public static string GetSelectedServerAddress(this RpcContext rpcContext)
        {
            var selectedServerHost = rpcContext.GetSelectedServerHost();
            if (selectedServerHost == null)
            {
                return null;
            }

            return $"{selectedServerHost}:{rpcContext.GetSelectedServerPort()}";
        }

        public static string GetSelectedServerHost(this RpcContext rpcContext)
        {
            var selectedServerHost = rpcContext.GetAttachment(AttachmentKeys.SelectedServerHost);
            return selectedServerHost?.ToString();
        }

        public static int GetSelectedServerPort(this RpcContext rpcContext)
        {
            var selectedServerPort = rpcContext.GetAttachment(AttachmentKeys.SelectedServerPort);
            return selectedServerPort.To<int>();
        }


        public static ServiceProtocol GetSelectedServerServiceProtocol(this RpcContext rpcContext)
        {
            var selectedServerServiceProtocol = rpcContext.GetAttachment(AttachmentKeys.SelectedServerServiceProtocol)
                .To<ServiceProtocol>();
            return selectedServerServiceProtocol;
        }

        public static string GetMessageId(this RpcContext rpcContext)
        {
            var messageId = rpcContext.GetAttachment(AttachmentKeys.MessageId);
            return messageId?.ToString();
        }
    }
}