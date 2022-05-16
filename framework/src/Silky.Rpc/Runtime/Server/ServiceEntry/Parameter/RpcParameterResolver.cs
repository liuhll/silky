using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class RpcParameterResolver : ParameterResolverBase
{
    public override object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        var parameters = serviceEntry.ConvertParameters(message.Parameters);
        return parameters;
    }
}