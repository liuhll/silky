namespace Silky.Rpc.Filters
{
    public interface IClientFilter : IClientFilterMetadata
    {
        void OnActionExecuting(ClientInvokeExecutingContext context);

        void OnActionExecuted(ClientInvokeExecutedContext context);
    }
}