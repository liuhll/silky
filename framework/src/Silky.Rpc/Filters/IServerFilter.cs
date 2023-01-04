namespace Silky.Rpc.Filters
{
    public interface IServerFilter : IServerFilterMetadata
    {
        void OnActionExecuting(ServerExecutingContext context);

        void OnActionExecuted(ServerExecutedContext context);
        
    }
}