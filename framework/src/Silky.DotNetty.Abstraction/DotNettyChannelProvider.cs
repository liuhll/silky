using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Core.DependencyInjection;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;

namespace Silky.DotNetty.Abstraction;

public class DotNettyChannelProvider : ITransientDependency, IChannelProvider
{
    private readonly IBootstrapProvider _bootstrapProvider;

    public DotNettyChannelProvider(IBootstrapProvider bootstrapProvider)
    {
        _bootstrapProvider = bootstrapProvider;
    }

    public async Task<IChannel> Create(IRpcEndpoint rpcEndpoint, ClientMessageListener messageListener,
        IRpcEndpointMonitor rpcEndpointMonitor)
    {
        var bootstrap = _bootstrapProvider.CreateClientBootstrap();
        var channel = await bootstrap.ConnectAsync(rpcEndpoint.IPEndPoint);
        var pipeline = channel.Pipeline;
        pipeline.AddLast("ClientHandler", new ClientHandler(messageListener, rpcEndpointMonitor));
        return channel;
    }
}