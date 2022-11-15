using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Codecs;
using DotNetty.Codecs.Compression;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Handlers;

public class SilkyClientChannelPoolHandler : IChannelPoolHandler
{
    private readonly X509Certificate2 _x509Certificate2;
    private readonly ITransportMessageDecoder _transportMessageDecoder;
    private readonly ITransportMessageEncoder _transportMessageEncoder;

    public ILogger<SilkyClientChannelPoolHandler> Logger { get; set; }

    public SilkyClientChannelPoolHandler(
        X509Certificate2 x509Certificate2,
        ITransportMessageDecoder transportMessageDecoder,
        ITransportMessageEncoder transportMessageEncoder)
    {
        _x509Certificate2 = x509Certificate2;
        _transportMessageDecoder = transportMessageDecoder;
        _transportMessageEncoder = transportMessageEncoder;
        Logger = EngineContext.Current.Resolve<ILogger<SilkyClientChannelPoolHandler>>() ??
                 NullLogger<SilkyClientChannelPoolHandler>.Instance;
    }

    public void ChannelReleased(IChannel channel)
    {
        Logger.LogDebug("ChannelReleased. Channel ID:" + channel.Id);
    }

    public void ChannelAcquired(IChannel channel)
    {
        Logger.LogDebug("ChannelAcquired. Channel ID:" + channel.Id);
    }

    public void ChannelCreated(IChannel channel)
    {
        var pipeline = channel.Pipeline;

        if (_x509Certificate2 != null)
        {
            var targetHost = _x509Certificate2.GetNameInfo(X509NameType.DnsName, false);
            pipeline.AddLast("tls",
                new TlsHandler(
                    stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true),
                    new ClientTlsSettings(targetHost)));
        }

        pipeline.AddLast(new LengthFieldPrepender(4));
        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
        pipeline.AddLast(ZlibCodecFactory.NewZlibEncoder(ZlibWrapper.Gzip));
        pipeline.AddLast(ZlibCodecFactory.NewZlibDecoder(ZlibWrapper.Gzip));
        pipeline.AddLast("encoder", new EncoderHandler(_transportMessageEncoder));
        pipeline.AddLast("decoder", new DecoderHandler(_transportMessageDecoder));
    }
}