using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IClientFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage);

        void OnActionExecuted(RemoteResultMessage context);
    }
}