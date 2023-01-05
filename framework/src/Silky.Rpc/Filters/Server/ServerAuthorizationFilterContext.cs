using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ServerAuthorizationFilterContext : FilterContext
{
    public ServerAuthorizationFilterContext(ServiceEntryContext context, 
        IList<IFilterMetadata> filters) 
        : base(context, filters)
    {
    }
    
    public object? Result { get; set; }
}