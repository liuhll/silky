using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using Lms.Rpc.Runtime.Server;

namespace Lms.DotNetty.Protocol.Ws.Handlers
{
    public class WebSocketServerHttpHandler : SimpleChannelInboundHandler2<IFullHttpRequest>
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public WebSocketServerHttpHandler(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = _serviceEntryLocator;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            if (!req.Result.IsSuccess)
            {
                SendHttpResponse(ctx, req,
                    new DefaultFullHttpResponse(req.ProtocolVersion, HttpResponseStatus.BadRequest,
                        ctx.Allocator.Buffer(0)));
                return;
            }

            if (!HttpMethod.Get.Equals(req.Method))
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(req.ProtocolVersion,
                    HttpResponseStatus.Forbidden,
                    ctx.Allocator.Buffer(0)));
                return;
            }

            var serviceEntry =
                _serviceEntryLocator.GetServiceEntryByApi(req.Uri, req.Method.ConvertAspNetCoreHttpMethod());
            if (serviceEntry == null)
            {
                var res = new DefaultFullHttpResponse(req.ProtocolVersion, HttpResponseStatus.NoContent,
                    ctx.Allocator.Buffer(0));
                SendHttpResponse(ctx, req, res);
                return;
            }

            if (serviceEntry.ServiceDescriptor.ServiceProtocol != ServiceProtocol.Ws)
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(req.ProtocolVersion,
                    HttpResponseStatus.BadRequest,
                    ctx.Allocator.Buffer(0)));
                return;
            }
            // IByteBuffer content = WebSocketServerBenchmarkPage.GetContent(GetWebSocketLocation(req, serviceEntry.ServiceDescriptor.Id));
            // var res = new DefaultFullHttpResponse(req.ProtocolVersion, OK, content);
            //
            // res.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
            // HttpUtil.SetContentLength(res, content.ReadableBytes);
            //
            // SendHttpResponse(ctx, req, res);
            
        }


        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            HttpResponseStatus responseStatus = res.Status;
            if (responseStatus.Code != 200)
            {
                ByteBufferUtil.WriteUtf8(res.Content, responseStatus.ToString());
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }

            // Send the response and close the connection if necessary.
            var keepAlive = HttpUtil.IsKeepAlive(req) && responseStatus.Code == 200;
            HttpUtil.SetKeepAlive(res, keepAlive);
            var future = ctx.WriteAndFlushAsync(res);
            if (!keepAlive)
            {
                future.CloseOnComplete(ctx.Channel);
            }
        }
    }
}