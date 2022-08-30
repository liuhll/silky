using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Codecs.Compression;
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
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Abstraction;

public class DotNettyChannelProvider : ITransientDependency, IChannelProvider
{
    private readonly IBootstrapProvider _bootstrapProvider;
    private GovernanceOptions _governanceOptions;
    private readonly ITransportMessageDecoder _transportMessageDecoder;
    private readonly ITransportMessageEncoder _transportMessageEncoder;

    public DotNettyChannelProvider(IBootstrapProvider bootstrapProvider,
        ITransportMessageDecoder transportMessageDecoder,
        ITransportMessageEncoder transportMessageEncoder,
        IOptionsMonitor<GovernanceOptions> governanceOptions)
    {
        _bootstrapProvider = bootstrapProvider;
        _transportMessageDecoder = transportMessageDecoder;
        _transportMessageEncoder = transportMessageEncoder;
        _governanceOptions = governanceOptions.CurrentValue;
        governanceOptions.OnChange((options, s) => _governanceOptions = options);
    }

    public async Task<IChannel> Create(IRpcEndpoint rpcEndpoint, ClientMessageListener messageListener,
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
                pipeline.AddLast(ZlibCodecFactory.NewZlibEncoder(ZlibWrapper.Gzip));
                pipeline.AddLast(ZlibCodecFactory.NewZlibDecoder(ZlibWrapper.Gzip));
                if (_governanceOptions.EnableHeartbeat &&
                    _governanceOptions.HeartbeatWatchIntervalSeconds > 0)
                {
                    pipeline.AddLast(new IdleStateHandler(
                        _governanceOptions.HeartbeatWatchIntervalSeconds * 2, 0,
                        0));
                    pipeline.AddLast(new ChannelInboundHandlerAdapter());
                }
                
                pipeline.AddLast("encoder",
                    new EncoderHandler(_transportMessageEncoder));
                pipeline.AddLast("decoder",
                    new DecoderHandler(_transportMessageDecoder));
            }));
        var channel = await bootstrap.ConnectAsync(rpcEndpoint.IPEndPoint);
        var pipeline = channel.Pipeline;
        pipeline.AddLast("ClientHandler", new ClientHandler(messageListener, rpcEndpointMonitor));
        return channel;
    }
}