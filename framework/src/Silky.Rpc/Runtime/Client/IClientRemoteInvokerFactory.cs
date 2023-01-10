using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Client;

public interface IClientRemoteInvokerFactory
{
    public IRemoteInvoker CreateInvoker(ClientInvokeContext context, ITransportClient client, string messageId);
}