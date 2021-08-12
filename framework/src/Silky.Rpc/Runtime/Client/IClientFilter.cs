using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IClientFilter : IScopedDependency
    {
        int Order { get; }

        void OnActionExecuting(ServiceEntryExecutingContext context);

        void OnActionExecuted(ServiceEntryExecutedContext context);
    }
}