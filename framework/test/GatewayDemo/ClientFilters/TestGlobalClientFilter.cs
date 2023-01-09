using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Rpc.Filters;

namespace GatewayDemo.ClientFilters;

public class TestGlobalClientFilter : IAsyncClientFilter
{

    public async Task OnActionExecutionAsync(ClientInvokeExecutingContext context, ClientInvokeExecutionDelegate next)
    {
        var logger = EngineContext.Current.Resolve<ILogger<TestGlobalClientFilter>>();
        logger.LogInformation("Before Invoke => ServiceEntryId: {ServiceEntryId}", context.RemoteInvokeMessage.ServiceEntryId);
        await next();
        logger.LogInformation("After Invoke => ServiceEntryId: {ServiceEntryId}", context.RemoteInvokeMessage.ServiceEntryId);
        
    }
}