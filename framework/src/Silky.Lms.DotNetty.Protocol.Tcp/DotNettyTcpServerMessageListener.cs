using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Lms.Core;
using Silky.Lms.DotNetty.Handlers;
using Silky.Lms.Rpc.Address;
using Silky.Lms.Rpc.Address.HealthCheck;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport;
using Silky.Lms.Rpc.Transport.Codec;
using Silky.Lms.Rpc.Utils;

namespace Silky.Lms.DotNetty.Protocol.Tcp
{
    public class DotNettyTcpServerMessageListener : MessageListenerBase, IServerMessageListener
    {
        public ILogger<DotNettyTcpServerMessageListener> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAddressModel _hostAddress;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly IHealthCheck _healthCheck;
        private IChannel m_boundChannel;
        private IEventLoopGroup m_bossGroup;
        private IEventLoopGroup m_workerGroup;

        public DotNettyTcpServerMessageListener(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment,
            ITransportMessageDecoder transportMessageDecoder,
            IHealthCheck healthCheck)
        {
            _hostEnvironment = hostEnvironment;
            _transportMessageDecoder = transportMessageDecoder;
            _healthCheck = healthCheck;
            _rpcOptions = rpcOptions.Value;
            _hostAddress = NetUtil.GetRpcAddressModel();
            if (_rpcOptions.IsSsl)
            {
                Check.NotNullOrEmpty(_rpcOptions.SslCertificateName, nameof(_rpcOptions.SslCertificateName));
            }

            Logger = NullLogger<DotNettyTcpServerMessageListener>.Instance;
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
                .Option(ChannelOption.SoBacklog, _rpcOptions.SoBacklog)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(m_bossGroup, m_workerGroup)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    if (tlsCertificate != null)
                    {
                        pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                    }

                    pipeline.AddLast(new LengthFieldPrepender(8));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 8, 0, 8));
                    pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                    pipeline.AddLast(new ServerHandler((channelContext, message) =>
                    {
                        if (message.IsInvokeMessage())
                        {
                            var sender = new DotNettyTcpServerMessageSender(channelContext);
                            OnReceived(sender, message);
                        }
                    }, _healthCheck));
                }));
            try
            {
                m_boundChannel = await bootstrap.BindAsync(_hostAddress.IPEndPoint);
                Logger.LogInformation($"服务监听者启动成功,监听地址:{_hostAddress},通信协议:{_hostAddress.ServiceProtocol}");
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"服务监听启动失败,监听地址:{_hostAddress},通信协议:{_hostAddress.ServiceProtocol},原因: {e.Message}");
                throw;
            }
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