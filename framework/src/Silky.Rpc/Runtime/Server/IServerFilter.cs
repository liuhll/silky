namespace Silky.Rpc.Runtime.Server
{
    public interface IServerFilter
    {
        int Order { get; }

        void OnActionExecuting(ServerExecutingContext context);

        void OnActionExecuted(ServerExecutedContext context);
    }
}