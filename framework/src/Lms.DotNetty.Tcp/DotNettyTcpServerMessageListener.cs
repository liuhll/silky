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
using Lms.Core;
using Lms.DotNetty.Tcp.Adapter;
using Lms.Rpc.Address;
using Lms.Rpc.Configuration;
using Lms.Rpc.Messages;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
using Lms.Rpc.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lms.DotNetty.Tcp
{
    public class DotNettyTcpServerMessageListener : MessageListenerBase
    {
        public ILogger<DotNettyTcpServerMessageListener> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IAddressModel _hostAddress;
        private IChannel boundChannel;

        public DotNettyTcpServerMessageListener(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _hostAddress = NetUtil.GetHostAddress(ServiceProtocol.Tcp);
            _rpcOptions = rpcOptions.Value;
            if (_rpcOptions.IsSsl)
            {
                Check.NotNullOrEmpty(_rpcOptions.SslCertificateName, nameof(_rpcOptions.SslCertificateName));
            }

            Logger = NullLogger<DotNettyTcpServerMessageListener>.Instance;
        }

        public override async Task Listen()
        {
            IEventLoopGroup bossGroup;
            IEventLoopGroup workerGroup;
            var bootstrap = new ServerBootstrap();
            if (_rpcOptions.UseLibuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();
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
                .Group(bossGroup, workerGroup)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    if (tlsCertificate != null)
                    {
                        pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                    }

                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new ServerHandler((channelContext, message) =>
                    {
                        if (message.IsInvokeMessage())
                        {
                            var sender = new DotNettyTcpServerMessageSender(channelContext);
                            OnReceived(sender, message);
                        }
                    }));
                }));
            try
            {
                boundChannel = await bootstrap.BindAsync(_hostAddress.IPEndPoint);
                Logger.LogInformation($"服务监听者启动成功,监听地址:{_hostAddress},通信协议:{_hostAddress.ServiceProtocol}");
            }
            catch (Exception e)
            {
                Logger.LogInformation($"服务监听启动失败,监听地址:{_hostAddress},通信协议:{_hostAddress.ServiceProtocol}");
                throw;
            }
        }
    }
}