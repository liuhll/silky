using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IClientResultFilter : IClientFilterMetadata
{
    void OnResultExecuting(ClientResultExecutingContext context);
    
    void OnResultExecuted(ClientResultExecutedContext context);
}