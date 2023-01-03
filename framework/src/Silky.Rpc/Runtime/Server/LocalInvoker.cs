using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

public class LocalInvoker : ILocalInvoker
{
    private readonly ILogger _logger;
    private readonly ServiceEntryContext _serviceEntryContext;
    private readonly IFilterMetadata[] _filters;
    private object _result;
    
    public LocalInvoker(ILogger logger, ServiceEntryContext serviceEntryContext,
        IFilterMetadata[] filters)
    {
        _logger = logger;
        _serviceEntryContext = serviceEntryContext;
        _filters = filters;
    }

    public async Task InvokeAsync()
    {
        
    }

    public object Result => _result;
}