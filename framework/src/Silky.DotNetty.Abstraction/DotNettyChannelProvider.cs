using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;

namespace Silky.DotNetty.Abstraction;

public class DotNettyChannelProvider : ITransientDependency, IChannelProvider
{
    private readonly IBootstrapProvider _bootstrapProvider;
    private GovernanceOptions _governanceOptions;

    public DotNettyChannelProvider(IBootstrapProvider bootstrapProvider,
        IOptionsMonitor<GovernanceOptions> governanceOptions)
    {
        _bootstrapProvider = bootstrapProvider;
        _governanceOptions = governanceOptions.CurrentValue;
        governanceOptions.OnChange((options, s) => _governanceOptions = options);
    }

    public async Task<IChannel> Create(ISilkyEndpoint silkyEndpoint, IMessageListener messageListener,
        IRpcEndpointMonitor rpcEndpointMonitor)
    {
        var bootstrap = _bootstrapProvider.CreateClientBootstrap();
        var tlsCertificate = _bootstrapProvider.GetX509Certificate2();
        bootstrap
            .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                if (tlsCertificate != null)
                {
                    var targetHost = tlsCertificate.GetNameInfo(X509NameType.DnsName, false);
                    pipeline.AddLast("tls",
                        new TlsHandler(
                            stream => new SslStream(stream, true,
                                (sender, certificate, chain, errors) => true),
                            new ClientTlsSettings(targetHost)));
                }

                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                // pipeline.AddLast(ZlibCodecFactory.NewZlibEncoder(ZlibWrapper.Gzip));
                // pipeline.AddLast(ZlibCodecFactory.NewZlibDecoder(ZlibWrapper.Gzip));
                if (_governanceOptions.EnableHeartbeat &&
                    _governanceOptions.HeartbeatWatchIntervalSeconds > 0)
                {
                    pipeline.AddLast(new IdleStateHandler(
                        _governanceOptions.HeartbeatWatchIntervalSeconds * 2, 0,
                        0));
                    pipeline.AddLast(new ChannelInboundHandlerAdapter());
                }
                
            }));
        var channel = await bootstrap.ConnectAsync(silkyEndpoint.IPEndPoint);
        var pipeline = channel.Pipeline;
        pipeline.AddLast("ClientHandler", new ClientHandler(messageListener, rpcEndpointMonitor));
        return channel;
    }
}