using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ResultExecutedContext : ServiceEntryContext
{
    public ResultExecutedContext(ServiceEntryContext context,object result) : base(context)
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