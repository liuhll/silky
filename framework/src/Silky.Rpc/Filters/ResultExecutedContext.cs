using System;
using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ResultExecutedContext : FilterContext
{
    public ResultExecutedContext(ServiceEntryContext context, IList<IFilterMetadata> filters, object result) :
        base(context, filters)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Result = result;
    }

    public virtual bool Canceled { get; set; }

    public virtual object Result { get; }
}