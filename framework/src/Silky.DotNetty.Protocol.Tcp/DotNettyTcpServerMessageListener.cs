﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Codec;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Protocol.Tcp
{
    public class DotNettyTcpServerMessageListener : MessageListenerBase, IServerMessageListener
    {
        public ILogger<DotNettyTcpServerMessageListener> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly IServer _server;
        private IChannel m_boundChannel;
        private IEventLoopGroup m_bossGroup;
        private IEventLoopGroup m_workerGroup;


        public DotNettyTcpServerMessageListener(IOptions<RpcOptions> rpcOptions,
            IHostEnvironment hostEnvironment,
            IServerProvider _serverProvider,
            ITransportMessageDecoder transportMessageDecoder,
            ITransportMessageEncoder transportMessageEncoder)
        {
            _hostEnvironment = hostEnvironment;
            _server = _serverProvider.GetServer();

            if (_server.RpcEndpoint == null)
            {
                throw new SilkyException("The current server host does not support SilkyRpc");
            }

            _transportMessageDecoder = transportMessageDecoder;
            _transportMessageEncoder = transportMessageEncoder;
            _rpcOptions = rpcOptions.Value;
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
                tlsCertificate = new X509Certificate2(GetCertificateFile(), _rpcOptions.SslCertificatePassword);
            }

            bootstrap
                .Option(ChannelOption.SoBacklog, _rpcOptions.SoBacklog)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(m_bossGroup, m_workerGroup)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    if (tlsCertificate != null)
                    {
                        pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                    }

                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    if (_rpcOptions.EnableHeartbeat && _rpcOptions.HeartbeatWatchIntervalSeconds > 0)
                    {
                        pipeline.AddLast(
                            new IdleStateHandler(0, _rpcOptions.HeartbeatWatchIntervalSeconds, 0));
                        pipeline.AddLast(
                            new ChannelInboundHandlerAdapter(EngineContext.Current.Resolve<IRpcEndpointMonitor>()));
                    }

                    pipeline.AddLast("encoder", new EncoderHandler(_transportMessageEncoder));
                    pipeline.AddLast("decoder", new DecoderHandler(_transportMessageDecoder));
                    pipeline.AddLast(new ServerHandler(async (channelContext, message) =>
                    {
                        if (message.IsInvokeMessage())
                        {
                            var sender = new DotNettyTcpServerMessageSender(channelContext);
                            await OnReceived(sender, message);
                        }
                    }));
                }));
            try
            {
                m_boundChannel = await bootstrap.BindAsync(_server.RpcEndpoint.IPEndPoint);
                Logger.LogInformation(
                    "Now Silky RPC server listening on: {0}", _server.RpcEndpoint.IPEndPoint);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw;
            }
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

        public async ValueTask DisposeAsync()
        {
            if (m_boundChannel != null) await m_boundChannel.CloseAsync();
            if (m_bossGroup != null)
                await m_bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            if (m_workerGroup != null)
                await m_workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }
}