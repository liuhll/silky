using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Abstraction;

public class SilkyChannelPoolMap : AbstractChannelPoolMap<IRpcEndpoint, FixedChannelPool>, ISingletonDependency
{
    private readonly ITransportMessageDecoder _transportMessageDecoder;
    private readonly ITransportMessageEncoder _transportMessageEncoder;
    private readonly IHostEnvironment _hostEnvironment;
    private RpcOptions _rpcOptions;
    private readonly SilkyClientChannelPoolHandler _silkyClientChannelPoolHandler;

    public SilkyChannelPoolMap(ITransportMessageDecoder transportMessageDecoder,
        ITransportMessageEncoder transportMessageEncoder,
        IOptions<RpcOptions> rpcOptions,
        IHostEnvironment hostEnvironment)
    {
        _transportMessageDecoder = transportMessageDecoder;
        _transportMessageEncoder = transportMessageEncoder;
        _hostEnvironment = hostEnvironment;
        _rpcOptions = rpcOptions.Value;

        X509Certificate2 tlsCertificate = null;
        string targetHost = null;
        if (_rpcOptions.IsSsl)
        {
            tlsCertificate =
                new X509Certificate2(GetCertificateFile(), _rpcOptions.SslCertificatePassword);
            targetHost = tlsCertificate.GetNameInfo(X509NameType.DnsName, false);
        }

        _silkyClientChannelPoolHandler =
            new SilkyClientChannelPoolHandler(tlsCertificate, targetHost, _transportMessageDecoder,
                _transportMessageEncoder);
    }

    public int TransportClientPoolNumber
    {
        get
        {
            var transportClientPoolNumber = _rpcOptions.TransportClientPoolNumber <= 1
                ? 1
                : _rpcOptions.TransportClientPoolNumber;
            return transportClientPoolNumber;
        }
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

        bootstrap
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_rpcOptions.ConnectTimeout))
            .Option(ChannelOption.TcpNodelay, true)
            .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
            .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Group(group)
            ;
        return bootstrap;
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

    protected override FixedChannelPool NewPool(IRpcEndpoint key)
    {
        var bootstrap = CreateBootstrap();
        var pool = new FixedChannelPool(bootstrap.RemoteAddress(key.IPEndPoint), _silkyClientChannelPoolHandler,
            ChannelActiveHealthChecker.Instance, FixedChannelPool.AcquireTimeoutAction.Fail,
            TimeSpan.FromMilliseconds(500), TransportClientPoolNumber, int.MaxValue);

        return pool;
    }
}