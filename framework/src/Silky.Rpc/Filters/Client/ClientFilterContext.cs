using System;
using System.Collections.Generic;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Filters;

public abstract class ClientFilterContext : ClientInvokeContext
{
    public ClientFilterContext(ClientInvokeContext context,IList<IFilterMetadata> filters) : base(context)
    {
        if (filters == null)
        {
            throw new ArgumentNullException(nameof(filters));
        }

        Filters = filters;
    }
    
    public virtual IList<IFilterMetadata> Filters { get; }
}