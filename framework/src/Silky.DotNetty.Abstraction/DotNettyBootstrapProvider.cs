using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;

namespace Silky.DotNetty.Abstraction;

internal class DotNettyBootstrapProvider : IDisposable, IBootstrapProvider, ITransientDependency
{
    private readonly RpcOptions _rpcOptions;
    private readonly IHostEnvironment _hostEnvironment;
    private IEventLoopGroup group;

    public DotNettyBootstrapProvider(IOptions<RpcOptions> rpcOptions,
        IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
        _rpcOptions = rpcOptions.Value;
    }

    public Bootstrap CreateClientBootstrap()
    {
        
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

    public X509Certificate2 GetX509Certificate2()
    {
        X509Certificate2 tlsCertificate = null;
        if (_rpcOptions.IsSsl)
        {
            tlsCertificate =
                new X509Certificate2(GetCertificateFile(), _rpcOptions.SslCertificatePassword);
        }

        return tlsCertificate;
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
        if (group != null)
        {
            await group.ShutdownGracefullyAsync();
        }
    }
}