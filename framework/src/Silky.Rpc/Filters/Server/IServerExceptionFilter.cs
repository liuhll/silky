using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IServerExceptionFilter : IServerFilterMetadata
{
    void OnException(ServerInvokeExceptionContext context);
}