using DotNetty.Transport.Channels.Pool;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;

namespace Silky.DotNetty.Abstraction;

public class SilkyChannelPoolMap : AbstractChannelPoolMap<ISilkyEndpoint, FixedChannelPool>, ISingletonDependency
{
    private readonly RpcOptions _rpcOptions;
    private readonly IBootstrapProvider _bootstrapProvider;

    public SilkyChannelPoolMap(
        IOptions<RpcOptions> rpcOptions,
        IBootstrapProvider bootstrapProvider)
    {
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


    protected override FixedChannelPool NewPool(ISilkyEndpoint key)
    {
        var bootstrap = _bootstrapProvider.CreateClientBootstrap();
        var tlsCertificate = _bootstrapProvider.GetX509Certificate2();
        var silkyClientChannelPoolHandler =
            new SilkyClientChannelPoolHandler(tlsCertificate);
        var pool = new FixedChannelPool(bootstrap.RemoteAddress(key.IPEndPoint), silkyClientChannelPoolHandler,
            TransportClientPoolNumber);
        return pool;
    }
}