using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels.Pool;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Abstraction;

public class ChannelPoolTransportClient : ITransportClient
{
    private readonly ITransportClientPool _transportClientPool;

    public ChannelPoolTransportClient(IChannelPool channelPool,
        IMessageListener messageListener,
        IRpcEndpointMonitor rpcEndpointMonitor,
        int transportClientPoolNumber)
    {
        _transportClientPool =
            new DefaultTransportClientPool(channelPool, messageListener, rpcEndpointMonitor,
                transportClientPoolNumber);
    }

    public async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
        int timeout = Timeout.Infinite)
    {
        var defaultTransportClient = await _transportClientPool.GetOrCreate();
        return await defaultTransportClient.SendAsync(message, messageId, timeout);
    }
}