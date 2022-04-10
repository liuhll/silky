using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class HttpParameterResolver : IParameterResolver
{
    public object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        var parameters = serviceEntry.ResolveParameters(message.HttpParameters);
        parameters = serviceEntry.ConvertParameters(parameters);
        return parameters;
    }
}