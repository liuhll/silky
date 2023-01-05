using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client;

public class ClientInvokeContext
{
    protected ClientInvokeContext(ClientInvokeContext context)
    {
        RemoteInvokeMessage = context.RemoteInvokeMessage;
        ShuntStrategy = context.ShuntStrategy;
        HashKey = context.HashKey;
    }
    
    public ClientInvokeContext(RemoteInvokeMessage remoteInvokeMessage, ShuntStrategy shuntStrategy,string hashKey)
    {
        RemoteInvokeMessage = remoteInvokeMessage;
        ShuntStrategy = shuntStrategy;
        HashKey = hashKey;
    }
    
    public RemoteInvokeMessage RemoteInvokeMessage { get; set; }

    public ShuntStrategy ShuntStrategy { get; set; }

    public string HashKey { get; set; }
}