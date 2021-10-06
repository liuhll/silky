using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IClientFilter
    {
        int Order { get; }

        void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage);

        void OnActionExecuted(RemoteResultMessage context);
    }
}