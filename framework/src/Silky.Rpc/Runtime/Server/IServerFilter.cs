using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(ServiceEntryExecutingContext context);

        void OnActionExecuted(ServiceEntryExecutedContext context);
    }
    
}