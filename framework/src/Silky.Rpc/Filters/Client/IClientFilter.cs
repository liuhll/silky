using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Filters
{
    public interface IClientFilter : IClientFilterMetadata, IOrderedFilter
    {
        int Order { get; }

        void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage);

        void OnActionExecuted(RemoteResultMessage context);
    }
}