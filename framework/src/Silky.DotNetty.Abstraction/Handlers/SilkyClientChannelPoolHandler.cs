using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Handlers;

public class SilkyClientChannelPoolHandler : IChannelPoolHandler
{
    private readonly SingleThreadEventLoop _workerGroup;
    private readonly X509Certificate2 _x509Certificate2;
    private readonly string _targetHost;
    private readonly ITransportMessageDecoder _transportMessageDecoder;
    private readonly ITransportMessageEncoder _transportMessageEncoder;


    public ILogger<SilkyClientChannelPoolHandler> Logger { get; set; }

    public SilkyClientChannelPoolHandler(
        X509Certificate2 x509Certificate2,
        string targetHost,
        ITransportMessageDecoder transportMessageDecoder,
        ITransportMessageEncoder transportMessageEncoder)
    {
        _workerGroup = new SingleThreadEventLoop();
        _x509Certificate2 = x509Certificate2;
        _targetHost = targetHost;
        _transportMessageDecoder = transportMessageDecoder;
        _transportMessageEncoder = transportMessageEncoder;
        Logger = NullLogger<SilkyClientChannelPoolHandler>.Instance;
    }

    public void ChannelReleased(IChannel channel)
    {
        Logger.LogInformation("ChannelReleased. Channel ID:" + channel.Id);
    }

    public void ChannelAcquired(IChannel channel)
    {
        Logger.LogInformation("ChannelAcquired. Channel ID:" + channel.Id);
    }

    public void ChannelCreated(IChannel channel)
    {
        var pipeline = channel.Pipeline;

        if (_x509Certificate2 != null)
        {
            pipeline.AddLast("tls",
                new TlsHandler(
                    stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true),
                    new ClientTlsSettings(_targetHost)));
        }

        pipeline.AddLast(new LengthFieldPrepender(4));
        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
        pipeline.AddLast(_workerGroup, "encoder", new EncoderHandler(_transportMessageEncoder));
        pipeline.AddLast(_workerGroup, "decoder", new DecoderHandler(_transportMessageDecoder));
    }
}