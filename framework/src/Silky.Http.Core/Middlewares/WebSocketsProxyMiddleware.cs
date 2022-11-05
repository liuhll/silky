using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Http.Core.Middlewares
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
        private readonly IServerManager _serverManager;
        private readonly WebSocketOptions _webSocketOptions;

        public WebSocketsProxyMiddleware(RequestDelegate next,
            IServerManager serverManager,
            IOptions<WebSocketOptions> webSocketOptions)
        {
            _next = next;
            _serverManager = serverManager;
            _webSocketOptions = webSocketOptions.Value;
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

            var serviceId = WebSocketResolverHelper.Generator(path);
            var rpcEndpoints = _serverManager.GetRpcEndpoints(serviceId, ServiceProtocol.Ws);

            if (rpcEndpoints == null)
            {
                throw new SilkyException($"The ws service with rpcEndpoint {path} does not exist.");
            }

            if (!rpcEndpoints.Any())
            {
                throw new SilkyException($"There is no available service with rpcEndpoint {path}.");
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

            client.Options.SetRequestHeader("wsToken", _webSocketOptions.Token);
            var businessId = GetKeyValue(context, "businessId");
            if (!businessId.IsNullOrEmpty())
            {
                client.Options.SetRequestHeader("businessId", businessId);
            }

            var hashKey = GetKeyValue(context, "hashKey");
            if (hashKey.IsNullOrEmpty())
            {
                if (!businessId.IsNullOrEmpty())
                {
                    hashKey = businessId;
                }
                else
                {
                    throw new SilkyException(
                        "When websocket establishes a session link, the hashkey or businessId must be specified through the header or qString");
                }
            }


            var addressSelector =
                EngineContext.Current.ResolveNamed<IRpcEndpointSelector>(ShuntStrategy.HashAlgorithm.ToString());
            var address = addressSelector.Select(new RpcEndpointSelectContext(serviceId,
                rpcEndpoints, hashKey));

            var destinationUri = CreateDestinationUri(address, path);
            await client.ConnectAsync(destinationUri, context.RequestAborted);
            using (var server = await context.WebSockets.AcceptWebSocketAsync(client.SubProtocol))
            {
                var bufferSize = DefaultWebSocketBufferSize;
                await Task.WhenAll(PumpWebSocket(client, server, bufferSize, context.RequestAborted),
                    PumpWebSocket(server, client, bufferSize, context.RequestAborted));
            }
        }

        private Uri CreateDestinationUri(ISilkyEndpoint silkyEndpoint, string path)
        {
            var scheme = "ws";
            if (_webSocketOptions.IsSsl)
            {
                scheme = "wss";
            }

            var wsAddress = $"{scheme}://{silkyEndpoint.Host}:{silkyEndpoint.Port}{path}";
            return new Uri(wsAddress);
        }

        private string GetKeyValue(HttpContext context, string key)
        {
            string val = null;
            if (context.Request.Headers.TryGetValue(key, out var headerKeyVal))
            {
                val = headerKeyVal.ToString();
            }

            if (context.Request.Query.TryGetValue(key, out var qStringKeyVal))
            {
                val = qStringKeyVal.ToString();
            }

            return val;
        }


        private static async Task PumpWebSocket(System.Net.WebSockets.WebSocket source,
            System.Net.WebSockets.WebSocket destination, int bufferSize,
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