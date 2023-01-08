using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public class ClientFilterAttribute :
    Attribute,
    IClientFilter,
    IAsyncClientFilter,
    IOrderedFilter,
    IClientResultFilter,
    IAsyncClientResultFilter,
    IClientFilterMetadata
{
    public int Order { get; set; }

    public virtual void OnActionExecuting(ClientInvokeExecutingContext context)
    {
    }

    public virtual void OnActionExecuted(ClientInvokeExecutedContext context)
    {
    }

    public virtual async Task OnActionExecutionAsync(ClientInvokeExecutingContext context,
        ClientInvokeExecutionDelegate next)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (next == null)
            throw new ArgumentNullException(nameof(next));
        this.OnActionExecuting(context);
        if (context.Result != null)
            return;
        this.OnActionExecuted(await next());
    }


    public virtual void OnResultExecuting(ClientResultExecutingContext context)
    {
    }

    public virtual void OnResultExecuted(ClientResultExecutedContext context)
    {
    }

    public virtual async Task OnResultExecutionAsync(ClientResultExecutingContext context,
        ClientResultExecutionDelegate next)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (next == null)
            throw new ArgumentNullException(nameof(next));
        this.OnResultExecuting(context);
        if (context.Cancel)
            return;
        this.OnResultExecuted(await next());
    }
}