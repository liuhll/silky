using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.DotNetty.Protocol.Ws.Handlers;
using Lms.Rpc.Address;
using Lms.Rpc.Address.HealthCheck;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
using Lms.Rpc.Transport.Codec;
using Lms.Rpc.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lms.DotNetty.Protocol.Ws
{
    public class DotNettyWsServerMessageListener : MessageListenerBase, IServerMessageListener
    {
        public ILogger<DotNettyWsServerMessageListener> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAddressModel _hostAddress;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ITypeFinder _typeFinder;
        private readonly IEnumerable<Type> _wsAppServiceTypes;
        private IChannel m_boundChannel;
        private IEventLoopGroup m_bossGroup;
        private IEventLoopGroup m_workerGroup;

        public DotNettyWsServerMessageListener(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment,
            ITransportMessageDecoder transportMessageDecoder,
            IHealthCheck healthCheck,
            IServiceEntryLocator serviceEntryLocator, 
            ITypeFinder typeFinder)
        {
            _hostEnvironment = hostEnvironment;
            _transportMessageDecoder = transportMessageDecoder;
            _healthCheck = healthCheck;
            _serviceEntryLocator = serviceEntryLocator;
            _typeFinder = typeFinder;
            _hostAddress = NetUtil.GetHostAddressModel(ServiceProtocol.Ws);
            _rpcOptions = rpcOptions.Value;
            _wsAppServiceTypes = _typeFinder.FindClassesOfType<WsAppServiceBase>().Where(p=> !p.IsAbstract);
            if (_rpcOptions.IsSsl)
            {
                Check.NotNullOrEmpty(_rpcOptions.SslCertificateName, nameof(_rpcOptions.SslCertificateName));
            }

            Logger = NullLogger<DotNettyWsServerMessageListener>.Instance;
        }

        public async Task Listen()
        {
            var bootstrap = new ServerBootstrap();
            if (_rpcOptions.UseLibuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                m_bossGroup = dispatcher;
                m_workerGroup = new WorkerEventLoopGroup(dispatcher);
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                m_bossGroup = new MultithreadEventLoopGroup(1);
                m_workerGroup = new MultithreadEventLoopGroup();
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
                .Group(m_bossGroup, m_workerGroup)
                .Option(ChannelOption.SoBacklog, _rpcOptions.SoBacklog)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (_rpcOptions.IsSsl)
                    {
                        pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                    }

                    pipeline.AddLast("idleStateHandler", new IdleStateHandler(0, 0, 120));
                    pipeline.AddLast(new HttpServerCodec());
                    pipeline.AddLast(new HttpObjectAggregator(65536));
                    pipeline.AddLast(new WebSocketServerCompressionHandler());
                    foreach (var wsAppServiceType in _wsAppServiceTypes)
                    {
                        var wsAppServiceWebSocketPath = GetWebSocketPathAppServiceType(wsAppServiceType);
                        pipeline.AddLast(new WebSocketServerProtocolHandler(
                            websocketPath: wsAppServiceWebSocketPath,
                            subprotocols: null,
                            allowExtensions: true,
                            maxFrameSize: 65536,
                            allowMaskMismatch: true,
                            checkStartsWith: false,
                            dropPongFrames: true,
                            enableUtf8Validator: false));
                        pipeline.AddLast(new WebSocketServerHttpHandler(_serviceEntryLocator));
                    }
                   
                    // pipeline.AddLast(new WebSocketFrameAggregator(65536));
                    // pipeline.AddLast(new WebSocketServerFrameHandler());
                }));
            
            var bootstrapChannel = await bootstrap.BindAsync(_hostAddress.IPEndPoint);
            async void DoBind()
            {
                await bootstrapChannel.CloseAsync();
                var ch = await bootstrap.BindAsync(_hostAddress.IPEndPoint);
                Interlocked.Exchange(ref bootstrapChannel, ch);
            }
        }

        private string GetWebSocketPathAppServiceType(Type wsAppServiceType)
        {
            var routeTemplateProvider =
                wsAppServiceType.CustomAttributes.OfType<IRouteTemplateProvider>().FirstOrDefault();
            if (routeTemplateProvider == null)
            {
                throw new LmsException("您必须要在应用接口中通过ServiceRoute特性标识服务");
            }

            if (routeTemplateProvider.ServiceProtocol != ServiceProtocol.Ws)
            {
                throw new LmsException("指定的服务协议不是WS");
            }

            return routeTemplateProvider.GetWsPath(wsAppServiceType.Name);
        }

        public async void Dispose()
        {
            if (m_boundChannel != null) await m_boundChannel.CloseAsync();
            if (m_bossGroup != null)
                await m_bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            if (m_workerGroup != null)
                await m_workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }
}