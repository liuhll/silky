namespace Silky.Rpc.Filters;

public interface IExceptionServerFilter : IServerFilterMetadata
{
    void OnException(ServerExceptionContext context);
}