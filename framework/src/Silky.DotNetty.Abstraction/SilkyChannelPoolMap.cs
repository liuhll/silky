using System;
using DotNetty.Transport.Channels.Pool;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Abstraction;

public class SilkyChannelPoolMap : AbstractChannelPoolMap<IRpcEndpoint, FixedChannelPool>, ISingletonDependency
{
    private readonly ITransportMessageDecoder _transportMessageDecoder;
    private readonly ITransportMessageEncoder _transportMessageEncoder;
    private readonly RpcOptions _rpcOptions;
    private readonly IBootstrapProvider _bootstrapProvider;

    public SilkyChannelPoolMap(ITransportMessageDecoder transportMessageDecoder,
        ITransportMessageEncoder transportMessageEncoder,
        IOptions<RpcOptions> rpcOptions,
        IBootstrapProvider bootstrapProvider)
    {
        _transportMessageDecoder = transportMessageDecoder;
        _transportMessageEncoder = transportMessageEncoder;
        _bootstrapProvider = bootstrapProvider;
        _rpcOptions = rpcOptions.Value;
    }

    private int TransportClientPoolNumber
    {
        get
        {
            var transportClientPoolNumber = _rpcOptions.TransportClientPoolNumber <= 50
                ? 50
                : _rpcOptions.TransportClientPoolNumber;
            return transportClientPoolNumber;
        }
    }


    protected override FixedChannelPool NewPool(IRpcEndpoint key)
    {
        var bootstrap = _bootstrapProvider.CreateClientBootstrap();
        var tlsCertificate = _bootstrapProvider.GetX509Certificate2();
        var silkyClientChannelPoolHandler =
            new SilkyClientChannelPoolHandler(tlsCertificate, _transportMessageDecoder, _transportMessageEncoder);
        var pool = new FixedChannelPool(bootstrap.RemoteAddress(key.IPEndPoint), silkyClientChannelPoolHandler,
            TransportClientPoolNumber);
        return pool;
    }
}