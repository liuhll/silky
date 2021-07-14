using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Runtime.Server.Filters
{
    public interface IServiceEntryFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(ServiceEntryExecutingContext context);

        void OnActionExecuted(ServiceEntryExecutedContext context);
    }
    
}