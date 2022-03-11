using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Silky.Rpc.Configuration;
using Silky.Rpc.Transport.Codec;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport;

namespace Silky.DotNetty
{
    public class DotNettyTransportClientFactory : IDisposable, ITransportClientFactory
    {
        private ConcurrentDictionary<IRpcEndpoint, Lazy<Task<ITransportClient>>> m_clients = new();

        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        private RpcOptions _rpcOptions;

        private GovernanceOptions _governanceOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public DotNettyTransportClientFactory(IOptions<RpcOptions> rpcOptions,
            IOptionsMonitor<GovernanceOptions> governanceOptions,
            IHostEnvironment hostEnvironment,
            ITransportMessageDecoder transportMessageDecoder,
            IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcOptions = rpcOptions.Value;
            _governanceOptions = governanceOptions.CurrentValue;
            governanceOptions.OnChange((options, s) => _governanceOptions = options);
            _hostEnvironment = hostEnvironment;
            _transportMessageDecoder = transportMessageDecoder;
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _rpcEndpointMonitor.OnDisEnable += async addressModel =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                m_clients.TryRemove(addressModel, out _);
            };
            _rpcEndpointMonitor.OnStatusChange += async (addressModel, health) =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                if (!health)
                {
                    m_clients.TryRemove(addressModel, out _);
                }
                else
                {
                    await GetOrCreateClient(addressModel);
                }
            };
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += async addressModel =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                m_clients.TryRemove(addressModel, out _);
            };
            Logger = NullLogger<DotNettyTransportClientFactory>.Instance;
        }

        private Bootstrap CreateBootstrap()
        {
            IEventLoopGroup group;
            var bootstrap = new Bootstrap();
            if (_rpcOptions.UseLibuv)
            {
                group = new EventLoopGroup();
            }
            else
            {
                group = new MultithreadEventLoopGroup();
            }

            X509Certificate2 tlsCertificate = null;
            string targetHost = null;
            if (_rpcOptions.IsSsl)
            {
                tlsCertificate = new X509Certificate2(GetCertificateFile(), _rpcOptions.SslCertificatePassword);
                targetHost = tlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            }

            var workerGroup = new SingleThreadEventLoop();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_rpcOptions.ConnectTimeout))
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group)
                .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                {
                    var pipeline = c.Pipeline;
                    if (tlsCertificate != null)
                    {
                        pipeline.AddLast("tls",
                            new TlsHandler(
                                stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true),
                                new ClientTlsSettings(targetHost)));
                    }

                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    if (_governanceOptions.EnableHeartbeat && _governanceOptions.HeartbeatWatchIntervalSeconds > 0)
                    {
                       
                        pipeline.AddLast(new IdleStateHandler(
                            _governanceOptions.HeartbeatWatchIntervalSeconds * 2, 0,
                            0));
                        pipeline.AddLast(new ChannelInboundHandlerAdapter());
                    }

                    pipeline.AddLast(workerGroup, new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                }));
            return bootstrap;
        }

        public async Task<ITransportClient> GetClient(IRpcEndpoint rpcEndpoint)
        {
            try
            {
                return await GetOrCreateClient(rpcEndpoint);
            }
            catch (Exception ex)
            {
                m_clients.TryRemove(rpcEndpoint, out _);
                Logger.LogException(ex);
                throw new CommunicationException(ex.Message, ex);
            }
        }

        private async Task<ITransportClient> GetOrCreateClient(IRpcEndpoint rpcEndpoint)
        {
            return await m_clients.GetOrAdd(rpcEndpoint
                , k => new Lazy<Task<ITransportClient>>(async () =>
                    {
                        Logger.LogInformation(
                            $"Ready to create a client for the server rpcEndpoint: {rpcEndpoint.IPEndPoint}.");
                        var bootstrap = CreateBootstrap();
                        var channel = await bootstrap.ConnectAsync(rpcEndpoint.IPEndPoint);
                        var pipeline = channel.Pipeline;
                        var messageListener = new ClientMessageListener();
                        var messageSender = new DotNettyClientMessageSender(channel);
                        pipeline.AddLast(new ClientHandler(messageListener, _rpcEndpointMonitor));
                        var client = new DefaultTransportClient(messageSender, messageListener);
                        return client;
                    }
                )).Value;
        }

        private string GetCertificateFile()
        {
            var certificateFileName = Path.Combine(_hostEnvironment.ContentRootPath, _rpcOptions.SslCertificateName);
            if (!File.Exists(certificateFileName))
            {
                certificateFileName =
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _rpcOptions.SslCertificateName);
            }

            if (!File.Exists(certificateFileName))
            {
                throw new SilkyException($"There is no ssl certificate for {certificateFileName}");
            }

            return certificateFileName;
        }

        public async void Dispose()
        {
            foreach (var client in m_clients.Values)
            {
                (client as IDisposable)?.Dispose();
            }
            // if (m_group != null)
            //     await m_group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }
}