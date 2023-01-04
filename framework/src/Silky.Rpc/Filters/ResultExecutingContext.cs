using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ResultExecutingContext : ServiceEntryContext
{
    public ResultExecutingContext(ServiceEntryContext context,object result) : base(context)
    {
        Result = result;
    }
    
    public virtual object Result { get; set; }
    
    public virtual bool Cancel { get; set; }
}