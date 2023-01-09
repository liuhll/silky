using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters
{
    public interface IServerFilter : IServerFilterMetadata
    {
        void OnActionExecuting(ServerInvokeExecutingContext context);

        void OnActionExecuted(ServerInvokeExecutedContext context);
        
    }
}