using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server.Filters
{
    public interface IServiceEntryFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(ServiceEntryExecutingContext context);

        void OnActionExecuted(ServiceEntryExecutedContext context);
    }
    
}