using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Address;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lms.WebSocketServer.Middleware
{
    public class WebSocketsProxyMiddleware
    {
        private static readonly string[] NotForwardedWebSocketHeaders = new[]
        {
            "Connection", "Host", "Upgrade", "Sec-WebSocket-Accept", "Sec-WebSocket-Protocol", "Sec-WebSocket-Key",
            "Sec-WebSocket-Version", "Sec-WebSocket-Extensions"
        };

        private const int DefaultWebSocketBufferSize = 4096;
        private const int StreamCopyBufferSize = 81920;
        private readonly RequestDelegate _next;
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly RpcOptions _rpcOptions;

        public WebSocketsProxyMiddleware(RequestDelegate next,
            ServiceRouteCache serviceRouteCache,
            IOptions<RpcOptions> rpcOptions)
        {
            _next = next;
            _serviceRouteCache = serviceRouteCache;
            _rpcOptions = rpcOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            await Proxy(context, path);
        }

        private async Task Proxy([NotNull] HttpContext context, [NotNull] string path)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(path, nameof(path));
            if (!context.WebSockets.IsWebSocketRequest)
            {
                throw new InvalidOperationException();
            }

            var serviceRoute = _serviceRouteCache.GetServiceRoute(WebSocketResolverHelper.Generator(path));

            if (serviceRoute == null)
            {
                throw new LmsException($"系统中不存在地址为{path}的ws服务");
            }

            if (!serviceRoute.Addresses.Any())
            {
                throw new LmsException($"系统中不存在地址为{path}的可用服务");
            }

            var client = new ClientWebSocket();
            foreach (var protocol in context.WebSockets.WebSocketRequestedProtocols)
            {
                client.Options.AddSubProtocol(protocol);
            }

            foreach (var headerEntry in context.Request.Headers)
            {
                if (!NotForwardedWebSocketHeaders.Contains(headerEntry.Key, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        client.Options.SetRequestHeader(headerEntry.Key, headerEntry.Value);
                    }
                    catch (ArgumentException)
                    {
                        // Expected in .NET Framework for headers that are mistakenly considered restricted.
                        // See: https://github.com/dotnet/corefx/issues/26627
                        // .NET Core does not exhibit this issue, ironically due to a separate bug (https://github.com/dotnet/corefx/issues/18784)
                    }
                }
            }

            var hashKey = GetHashKey(context);
            if (hashKey.IsNullOrEmpty())
            {
                throw new LmsException("websocket在建立会话链接时,必须通过header或是qString指定hashkey");
            }

            var addressSelector =
                EngineContext.Current.ResolveNamed<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
            var address = addressSelector.Select(new AddressSelectContext(serviceRoute.ServiceDescriptor.Id,
                serviceRoute.Addresses, hashKey));

            var destinationUri = CreateDestinationUri(address, path);
            await client.ConnectAsync(destinationUri, context.RequestAborted);
            using (var server = await context.WebSockets.AcceptWebSocketAsync(client.SubProtocol))
            {
                var bufferSize = DefaultWebSocketBufferSize;
                await Task.WhenAll(PumpWebSocket(client, server, bufferSize, context.RequestAborted), PumpWebSocket(server, client, bufferSize, context.RequestAborted));
            }
        }

        private Uri CreateDestinationUri(IAddressModel address, string path)
        {
            var scheme = "ws";
            if (_rpcOptions.IsSsl)
            {
                scheme = "wss";
            }

            var wsAddress = $"{scheme}://{address.Address}:{address.Port}{path}";
            return new Uri(wsAddress);
        }

        private string GetHashKey(HttpContext context)
        {
            string hashKey = null;
            if (context.Request.Headers.TryGetValue("hashKey", out var headerhashKeyVal))
            {
                hashKey = headerhashKeyVal.ToString();
            }

            if (context.Request.Query.TryGetValue("hashKey", out var queryHashKeyVal))
            {
                hashKey = queryHashKeyVal.ToString();
            }

            return hashKey;
        }


        private static async Task PumpWebSocket(WebSocket source, WebSocket destination, int bufferSize,
            CancellationToken cancellationToken)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            var buffer = new byte[bufferSize];
            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await destination.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null,
                        cancellationToken);
                    return;
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        await destination.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null,
                            cancellationToken);
                        return;
                    }

                    throw;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await destination.CloseOutputAsync(source.CloseStatus.Value, source.CloseStatusDescription,
                        cancellationToken);
                    return;
                }

                await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType,
                    result.EndOfMessage, cancellationToken);
            }
        }
    }
}