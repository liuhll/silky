using System.Collections.Generic;
using JetBrains.Annotations;
using Silky.Core.Extensions;
using Silky.Core.Serialization;

namespace Silky.Core.Runtime.Rpc
{
    public static class RpcContextExtensions
    {
        public static bool IsGateway(this RpcContext rpcContext)
        {
            var isGateway = rpcContext.GetInvokeAttachment(AttachmentKeys.IsGateway);
            if (isGateway == null)
            {
                return false;
            }

            return isGateway.To<bool>();
        }

        public static string GetClientHost(this RpcContext rpcContext)
        {
            var clientHost = rpcContext.GetInvokeAttachment(AttachmentKeys.ClientHost);
            return clientHost?.ToString();
        }

        public static int GetClientPort(this RpcContext rpcContext)
        {
            var clientPort = rpcContext.GetInvokeAttachment(AttachmentKeys.ClientPort);
            return clientPort.To<int>();
        }

        public static int? GetRpcRequestPort(this RpcContext rpcContext)
        {
            var rpcRequestPort = rpcContext.GetInvokeAttachment(AttachmentKeys.RpcRequestPort);
            if (rpcRequestPort != null)
            {
                return rpcRequestPort.To<int>();
            }

            return null;
        }

        public static ServiceProtocol GetClientServiceProtocol(this RpcContext rpcContext)
        {
            var clientPort = rpcContext.GetInvokeAttachment(AttachmentKeys.ClientServiceProtocol);
            return clientPort.To<ServiceProtocol>();
        }


        public static string GetLocalHost(this RpcContext rpcContext)
        {
            var localAddress = rpcContext.GetInvokeAttachment(AttachmentKeys.LocalAddress);
            return localAddress;
        }

        public static int GetLocalPort(this RpcContext rpcContext)
        {
            var localPort = rpcContext.GetInvokeAttachment(AttachmentKeys.LocalPort);
            return localPort.To<int>();
        }

        public static ServiceProtocol GetLocalServiceProtocol(this RpcContext rpcContext)
        {
            var localServiceProtocol = rpcContext.GetInvokeAttachment(AttachmentKeys.LocalServiceProtocol);
            return localServiceProtocol.To<ServiceProtocol>();
        }


        public static string GetServiceKey(this RpcContext rpcContext)
        {
            var serviceKey = rpcContext.GetInvokeAttachment(AttachmentKeys.ServiceKey);
            return serviceKey?.ToString();
        }


        public static void SetServiceKey(this RpcContext rpcContext, string serviceKey)
        {
            rpcContext.SetInvokeAttachment(AttachmentKeys.ServiceKey, serviceKey);
        }

        public static void SetHashKey(this RpcContext rpcContext, string hashKey)
        {
            var headers = rpcContext.GetRequestHeader() ?? new Dictionary<string, string>();
            headers[AttachmentKeys.HashKey] = hashKey;
            rpcContext.SetRequestHeader(headers);
        }

        public static void SetRequestHeader(this RpcContext rpcContext, IDictionary<string, string> headers)
        {
            rpcContext.SetInvokeAttachment(AttachmentKeys.RequestHeader, headers);
        }

        public static IDictionary<string, string> GetRequestHeader(this RpcContext rpcContext)
        {
            var requestHeaders = rpcContext.GetInvokeAttachment(AttachmentKeys.RequestHeader);
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            return serializer.Deserialize<Dictionary<string, string>>(requestHeaders);
        }

        public static void SetRequestParameters(this RpcContext rpcContext, string requestParameters)
        {
            rpcContext.SetInvokeAttachment(AttachmentKeys.RequestParameters, requestParameters);
        }

        public static string GetRequestParameters(this RpcContext rpcContext)
        {
            return rpcContext.GetInvokeAttachment(AttachmentKeys.RequestParameters)?.ToString();
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
            var selectedServerHost = rpcContext.GetInvokeAttachment(AttachmentKeys.SelectedServerHost);
            return selectedServerHost?.ToString();
        }

        public static int GetSelectedServerPort(this RpcContext rpcContext)
        {
            var selectedServerPort = rpcContext.GetInvokeAttachment(AttachmentKeys.SelectedServerPort);
            return selectedServerPort.To<int>();
        }


        public static ServiceProtocol GetSelectedServerServiceProtocol(this RpcContext rpcContext)
        {
            var selectedServerServiceProtocol = rpcContext
                .GetInvokeAttachment(AttachmentKeys.SelectedServerServiceProtocol)
                .To<ServiceProtocol>();
            return selectedServerServiceProtocol;
        }

        public static string GetMessageId(this RpcContext rpcContext)
        {
            var messageId = rpcContext.GetInvokeAttachment(AttachmentKeys.MessageId);
            return messageId?.ToString();
        }

        public static void SigninToSwagger(this RpcContext rpcContext, [NotNull] string token)
        {
            rpcContext.SetResponseHeader("access-token", token);
        }

        public static void SignoutToSwagger(this RpcContext rpcContext)
        {
            rpcContext.SetResponseHeader("access-token", "invalid_token");
        }
    }
}