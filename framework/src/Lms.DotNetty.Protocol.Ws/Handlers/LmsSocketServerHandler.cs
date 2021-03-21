using System.Collections.Generic;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using JetBrains.Annotations;
using Lms.Core;

namespace Lms.DotNetty.Protocol.Ws.Handlers
{
    public class LmsWebSocketServerHttpHandler : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        private readonly IEnumerable<string> _wsPaths;
        private readonly bool _isSsl;
        private WebSocketServerHandshaker m_handshaker;


        public LmsWebSocketServerHttpHandler(IEnumerable<string> wsPaths, bool isSsl)
        {
            _wsPaths = wsPaths;
            _isSsl = isSsl;
        }

        // protected async override void ChannelRead0(IChannelHandlerContext ctx, TransportMessage msg)
        // {
        //     var remoteInvokeMessage = msg.GetContent<RemoteInvokeMessage>();
        //
        //     if (remoteInvokeMessage.Attachments.TryGetValue("uri", out var uri)
        //         && remoteInvokeMessage.Attachments.TryGetValue("requestHeader", out var requestHeader))
        //     {
        //         //var host = ((Dictionary<string, object>) requestHeader)["host"];
        //         var host = NetUtil.GetHostAddress("0.0.0.0");
        //         var wsUri = uri.ToString();
        //         var wsFactory =
        //             new WebSocketServerHandshakerFactory(GetWebSocketLocation(host.ToString(), wsUri), null, true,
        //                 5 * 1024 * 1024);
        //         var request = new DefaultFullHttpRequest(HttpVersion.Http11, HttpMethod.Get, wsUri);
        //         m_handshaker = wsFactory.NewHandshaker(request);
        //
        //         if (m_handshaker == null)
        //         {
        //             await WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
        //         }
        //         else
        //         {
        //             await m_handshaker.HandshakeAsync(ctx.Channel, request);
        //         }
        //     }
        // }

        // protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest request)
        // {
        //     if (!_wsPaths.Any(p => p == request.Uri))
        //     {
        //         SendHttpResponse(ctx, request,
        //             new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.NotFound));
        //         return;
        //     }
        //
        //     var wsFactory = new WebSocketServerHandshakerFactory(
        //         GetWebSocketLocation(request), null, true, 5 * 1024 * 1024);
        //     m_handshaker = wsFactory.NewHandshaker(request);
        //     if (m_handshaker == null)
        //     {
        //         WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
        //     }
        //     else
        //     {
        //         m_handshaker.HandshakeAsync(ctx.Channel, request);
        //     }
        // }

        // static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        // {
        //     // Generate an error page if response getStatus code is not OK (200).
        //     if (res.Status.Code != 200)
        //     {
        //         IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
        //         res.Content.WriteBytes(buf);
        //         buf.Release();
        //         HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
        //     }
        //
        //     // Send the response and close the connection if necessary.
        //     Task task = ctx.Channel.WriteAndFlushAsync(res);
        //     if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
        //     {
        //         task.ContinueWith((t, c) => ((IChannelHandlerContext) c).CloseAsync(),
        //             ctx, TaskContinuationOptions.ExecuteSynchronously);
        //     }
        // }
        //
        private string GetWebSocketLocation([NotNull] string host, [NotNull] string uri)
        {
            Check.NotNull(host, nameof(host));
            Check.NotNull(uri, nameof(host));
            string location = host + uri;

            if (_isSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {
            throw new System.NotImplementedException();
        }
    }
}