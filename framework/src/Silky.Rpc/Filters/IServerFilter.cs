namespace Silky.Rpc.Filters
{
    public interface IServerFilter : IFilterMetadata
    {
        void OnActionExecuting(ServerExecutingContext context);

        void OnActionExecuted(ServerExecutedContext context);
        
    }
}