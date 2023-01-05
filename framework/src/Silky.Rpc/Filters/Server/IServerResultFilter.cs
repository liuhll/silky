namespace Silky.Rpc.Filters;

public interface IServerResultFilter : IServerFilterMetadata
{
    void OnResultExecuting(ServerResultExecutingContext context);
    
    void OnResultExecuted(ServerResultExecutedContext context);
}