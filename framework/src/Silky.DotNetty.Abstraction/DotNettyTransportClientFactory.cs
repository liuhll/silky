using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Configuration;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Codec;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Runtime.Client;

namespace Silky.DotNetty
{
    public class DotNettyTransportClientFactory : IDisposable, ITransportClientFactory
    {
        private ConcurrentDictionary<IAddressModel, Lazy<Task<ITransportClient>>> m_clients = new();

        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        private readonly Bootstrap _bootstrap;
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly IHealthCheck _healthCheck;
        // private IEventLoopGroup m_group;

        public DotNettyTransportClientFactory(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment,
            ITransportMessageDecoder transportMessageDecoder,
            IHealthCheck healthCheck)
        {
            _rpcOptions = rpcOptions.Value;
            _hostEnvironment = hostEnvironment;
            _transportMessageDecoder = transportMessageDecoder;
            _healthCheck = healthCheck;
            _healthCheck.OnUnhealth += async addressModel =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                m_clients.TryRemove(addressModel, out var remoteModule);
            };
            _healthCheck.OnHealthChange += async (addressModel, health) =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                if (!health)
                {
                    m_clients.TryRemove(addressModel, out var remoteModule);
                }
                else
                {
                    await GetOrCreateClient(addressModel);
                }
            };
            _healthCheck.OnRemveAddress += async addressModel =>
            {
                Check.NotNull(addressModel, nameof(addressModel));
                m_clients.TryRemove(addressModel, out var remoteModule);
            };

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
            }
            else
            {
                group = new MultithreadEventLoopGroup();
            }

            X509Certificate2 tlsCertificate = null;
            string targetHost = null;
            if (_rpcOptions.IsSsl)
            {
                tlsCertificate =
                    new X509Certificate2(Path.Combine(_hostEnvironment.ContentRootPath, _rpcOptions.SslCertificateName),
                        _rpcOptions.SslCertificatePassword);
                targetHost = tlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            }

            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_rpcOptions.ConnectTimeout))
                .Option(ChannelOption.TcpNodelay, true)
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

                    pipeline.AddLast(new LengthFieldPrepender(8));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 8, 0, 8));
                    pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                }));
            return bootstrap;
        }

        public async Task<ITransportClient> GetClient(IAddressModel addressModel)
        {
            try
            {
                return await GetOrCreateClient(addressModel);
            }
            catch (Exception ex)
            {
                m_clients.TryRemove(addressModel, out var value);
                throw;
            }
        }

        private async Task<ITransportClient> GetOrCreateClient(IAddressModel addressModel)
        {
            return await m_clients.GetOrAdd(addressModel
                , k => new Lazy<Task<ITransportClient>>(async () =>
                    {
                        Logger.LogInformation($"Ready to create a client for the server address: {addressModel.IPEndPoint}" +
                                              $"");
                        var bootstrap = _bootstrap;
                        var channel = await bootstrap.ConnectAsync(k.IPEndPoint);
                        var pipeline = channel.Pipeline;
                        var messageListener = new ClientMessageListener();
                        var messageSender = new DotNettyClientMessageSender(channel);
                        pipeline.AddLast(new ClientHandler(messageListener, _healthCheck));
                        var client = new DefaultTransportClient(messageSender, messageListener);
                        return client;
                    }
                )).Value;
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