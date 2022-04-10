using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public interface IParameterResolver
{
    object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message);
}