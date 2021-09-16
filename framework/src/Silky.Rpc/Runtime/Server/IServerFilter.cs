using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(ServerExecutingContext context);

        void OnActionExecuted(ServerExecutedContext context);
    }
    
}