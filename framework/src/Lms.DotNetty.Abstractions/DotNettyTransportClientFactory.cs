using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Lms.DotNetty.Adapter;
using Lms.Rpc.Configuration;
using Lms.Rpc.Transport;
using Lms.Rpc.Transport.Codec;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lms.DotNetty
{
    public class DotNettyTransportClientFactory : ITransportClientFactory
    {
        private ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> m_clients =
            new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();

        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        private readonly Bootstrap _bootstrap;
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public DotNettyTransportClientFactory(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment,
            ITransportMessageDecoder transportMessageDecoder)
        {
            _rpcOptions = rpcOptions.Value;
            _hostEnvironment = hostEnvironment;
            _transportMessageDecoder = transportMessageDecoder;
            _bootstrap = CreateBootstrap();
            Logger = NullLogger<DotNettyTransportClientFactory>.Instance;
        }

        private Bootstrap CreateBootstrap()
        {
            IEventLoopGroup group;

            var bootstrap = new Bootstrap();
            if (_rpcOptions.UseLibuv)
            {
                group = new EventLoopGroup();
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                group = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }

            X509Certificate2 tlsCertificate = null;
            if (_rpcOptions.IsSsl)
            {
                tlsCertificate =
                    new X509Certificate2(Path.Combine(_hostEnvironment.ContentRootPath, _rpcOptions.SslCertificateName),
                        _rpcOptions.SslCertificatePassword);
            }

            bootstrap
                .Channel<TcpSocketChannel>()
                // .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_rpcOptions.ConnectTimeout))
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group)
                .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                {
                    var pipeline = c.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(8));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 8, 0, 8));
                    pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                }));
            return bootstrap;
        }

        public async Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            try
            {
                return await m_clients.GetOrAdd(endPoint
                    , k => new Lazy<Task<ITransportClient>>(async () =>
                        {
                            Logger.LogInformation($"准备为服务端地址：{endPoint}创建客户端");
                            var bootstrap = _bootstrap;
                            var channel = await bootstrap.ConnectAsync(k);
                            var pipeline = channel.Pipeline;
                            var messageListener = new ClientMessageListener();
                            var messageSender = new DotNettyClientMessageSender(channel);
                            pipeline.AddLast(new ClientHandler(messageListener,messageSender));
                            var client = new DefaultTransportClient(messageSender, messageListener);
                            return client;
                        }
                    )).Value;
            }
            catch (Exception ex)
            {
                //移除
                m_clients.TryRemove(endPoint, out var value);
                throw;
            }
        }
    }
}