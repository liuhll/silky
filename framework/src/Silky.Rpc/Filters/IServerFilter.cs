namespace Silky.Rpc.Filters
{
    public interface IServerFilter : IFilterMetadata
    {
        int Order { get; }

        void OnActionExecuting(ServerExecutingContext context);

        void OnActionExecuted(ServerExecutedContext context);

        void OnActionException(ServerExceptionContext context);  
    }
}