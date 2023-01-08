using System.Threading.Tasks;
using Silky.Core.Convertible;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client.Filters;

public class AsyncAlwaysRunRemoteResultFilter : IAsyncAlwaysRunClientResultFilter
{
    private readonly IServiceEntryLocator _serviceEntryLocator;
    private readonly ITypeConvertibleService _typeConvertibleService;

    public AsyncAlwaysRunRemoteResultFilter(IServiceEntryLocator serviceEntryLocator,
        ITypeConvertibleService typeConvertibleService)
    {
        _serviceEntryLocator = serviceEntryLocator;
        _typeConvertibleService = typeConvertibleService;
    }

    public async Task OnResultExecutionAsync(ClientResultExecutingContext context, ClientResultExecutionDelegate next)
    {
        var remoteResult = context.Result;
        var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteResult.ServiceEntryId);
        if (serviceEntry != null)
        {
            if (remoteResult.Result != null && remoteResult.Result.GetType() != serviceEntry.ReturnType)
            {
                context.Result.Result = _typeConvertibleService.Convert(remoteResult.Result, serviceEntry.ReturnType);
            }
        }

        await next();
    }
}