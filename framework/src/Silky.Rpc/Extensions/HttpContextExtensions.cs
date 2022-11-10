using System.Net;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Runtime.Rpc;
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
        
        public static ServiceEntryDescriptor GetServiceEntryDescriptor(this HttpContext context)
        {
            var serviceEntryDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ServiceEntryDescriptor>();
            return serviceEntryDescriptor;
        }

        public static void SignoutToSwagger(this HttpContext httpContext)
        {
            httpContext.Response.Headers["access-token"] = "invalid_token";
        }

        public static void SetHttpMessageId(this HttpContext httpContext)
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.MessageId, httpContext.TraceIdentifier);
        }

        public static void SetUserClaims(this HttpContext httpContext)
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                foreach (var userClaim in httpContext.User.Claims)
                {
                    RpcContext.Context.SetInvokeAttachment(userClaim.Type, userClaim.Value);
                }
            }
        }

        public static void SetHttpHandleAddressInfo(this HttpContext httpContext)
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.IsGateway, "true");
            
            var clientHost = httpContext.Connection.RemoteIpAddress;
            var clientPort = httpContext.Connection.RemotePort;

            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ClientHost, clientHost.MapToIPv4().ToString());
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ClientServiceProtocol,
                ServiceProtocol.Http.ToString());
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RpcRequestPort, clientPort.ToString());
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ClientPort, clientPort.ToString());

            var localRpcEndpoint = SilkyEndpointHelper.GetLocalWebEndpointDescriptor();
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalAddress, localRpcEndpoint.Host);
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalPort, localRpcEndpoint.Port.ToString());
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalServiceProtocol,
                localRpcEndpoint.ServiceProtocol.ToString());
        }

        public static void SetResultStatusCode(this HttpResponse httpResponse, StatusCode statusCode)
        {
            httpResponse.Headers["SilkyResultStatusCode"] = statusCode.ToString();
        }
        
        public static StatusCode? GetResultStatusCode(this HttpResponse httpResponse)
        {
            if (httpResponse.Headers.Keys.Contains("SilkyResultStatusCode"))
            {
                return httpResponse.Headers["SilkyResultStatusCode"].To<StatusCode>();
            }
            return null;
        }
        
        public static void SetResultStatus(this HttpResponse httpResponse, int status)
        {
            httpResponse.Headers["SilkyResultStatus"] = status.ToString();
        }

        public static void SetResultStatusCode(this IHeaderDictionary destination, StatusCode statusCode)
        {
            destination["SilkyResultStatusCode"] = statusCode.ToString();
        }
        
        public static void SetResultStatus(this IHeaderDictionary destination, int status)
        {
            destination["SilkyResultStatus"] = status.ToString();
        }

        public static void SetHeaders(this HttpResponse httpResponse)
        {
            var responseHeaders = RpcContext.Context.GetResponseHeaders();
            if (responseHeaders != null)
            {
                foreach (var responseHeader in responseHeaders)
                {
                    httpResponse.Headers[responseHeader.Key] = responseHeader.Value;
                }
            }
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
                case HttpStatusCode.OK:
                    statusCode = StatusCode.Success;
                    break;
                case HttpStatusCode.Unauthorized:
                    statusCode = StatusCode.UnAuthentication;
                    break;
                case HttpStatusCode.Forbidden:
                    statusCode = StatusCode.UnAuthorization;
                    break;
                case HttpStatusCode.NotFound:
                    statusCode = StatusCode.NotFound;
                    break;
                case HttpStatusCode.NoContent:
                    statusCode = StatusCode.NoContent;
                    break;
                case HttpStatusCode.MethodNotAllowed:
                    statusCode = StatusCode.RouteParseError;
                    break;
                default:
                    statusCode = StatusCode.BadRequest;
                    break;
            }

            return statusCode;
        }
    }
}