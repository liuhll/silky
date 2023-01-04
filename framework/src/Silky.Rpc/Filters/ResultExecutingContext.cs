using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ResultExecutingContext : FilterContext
{
    public ResultExecutingContext(ServiceEntryContext context, IList<IFilterMetadata> filters, object result) : base(
        context, filters)
    {
        Result = result;
    }

    public virtual object Result { get; set; }

    public virtual bool Cancel { get; set; }
}