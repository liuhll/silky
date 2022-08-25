using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;

namespace Silky.DotNetty.Abstraction;

public interface IChannelProvider 
{
    Task<IChannel> Create(IRpcEndpoint rpcEndpoint, ClientMessageListener messageListener, IRpcEndpointMonitor rpcEndpointMonitor);
}