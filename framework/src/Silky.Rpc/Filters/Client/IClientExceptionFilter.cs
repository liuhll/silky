using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IClientExceptionFilter : IClientFilterMetadata
{
    void OnException(ClientInvokeExceptionContext context);
}