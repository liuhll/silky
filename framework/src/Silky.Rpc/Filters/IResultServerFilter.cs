namespace Silky.Rpc.Filters;

public interface IResultServerFilter : IServerFilterMetadata
{
    void OnResultExecuting(ResultExecutingContext context);
    
    void OnResultExecuted(ResultExecutedContext context);
}