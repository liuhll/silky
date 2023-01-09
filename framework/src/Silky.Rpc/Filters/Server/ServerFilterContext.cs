using System;
using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public abstract class ServerFilterContext : ServiceEntryContext
{
    public ServerFilterContext(ServiceEntryContext context,IList<IFilterMetadata> filters) : base(context)
    {
        if (filters == null)
        {
            throw new ArgumentNullException(nameof(filters));
        }

        Filters = filters;
    }
    
    public virtual IList<IFilterMetadata> Filters { get; }
}