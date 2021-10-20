using System.Net;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class HttpContextExtensions
    {
        public static ServiceEntry GetServiceEntry(this HttpContext context)
        {
            var serviceEntry = context.GetEndpoint()?.Metadata.GetMetadata<ServiceEntry>();
            return serviceEntry;
        }

        public static void SignoutToSwagger(this HttpContext httpContext)
        {
            httpContext.Response.Headers["access-token"] = "invalid_token";
        }

        public static void SetHttpMessageId(this HttpContext httpContext)
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.MessageId, httpContext.TraceIdentifier);
        }

        public static void SetUserClaims(this HttpContext httpContext)
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                foreach (var userClaim in httpContext.User.Claims)
                {
                    RpcContext.Context.SetAttachment(userClaim.Type, userClaim.Value);
                }
            }
        }

        public static void SetHttpHandleAddressInfo(this HttpContext httpContext)
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.IsGateway, true);

          
            var clientHost = httpContext.Connection.RemoteIpAddress;
            var clientPort = httpContext.Connection.RemotePort;

            RpcContext.Context.SetAttachment(AttachmentKeys.ClientHost, clientHost.MapToIPv4().ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.ClientServiceProtocol, ServiceProtocol.Http.ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.RpcRequestPort, clientPort.ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.ClientPort, clientPort.ToString());

            var localRpcEndpoint = RpcEndpointHelper.GetLocalWebEndpointDescriptor();
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalAddress, localRpcEndpoint.Host);
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalPort, localRpcEndpoint.Port);
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalServiceProtocol, localRpcEndpoint.ServiceProtocol);
        }

        public static void SetResultCode(this HttpResponse httpResponse, StatusCode statusCode)
        {
            httpResponse.Headers["SilkyResultCode"] = statusCode.ToString();
        }

        public static StatusCode GetResultCode(this HttpResponse httpResponse, int httpCode)
        {
            var silkyResultCode = httpResponse.Headers["SilkyResultCode"];
            if (!silkyResultCode.IsNullOrEmpty())
            {
                return silkyResultCode.ToString().To<StatusCode>();
            }

            StatusCode statusCode;
            switch ((HttpStatusCode)httpCode)
            {
                case HttpStatusCode.Unauthorized:
                    statusCode = StatusCode.UnAuthentication;
                    break;
                case HttpStatusCode.NoContent:
                    statusCode = StatusCode.NoContent;
                    break;
                case HttpStatusCode.MethodNotAllowed:
                    statusCode = StatusCode.RouteParseError;
                    break;
                default:
                    statusCode = StatusCode.ServerError;
                    break;
            }

            return statusCode;
        }
    }
}