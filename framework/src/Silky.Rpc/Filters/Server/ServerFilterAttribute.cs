using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Filters
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public abstract class ServerFilterAttribute :
        Attribute,
        IServerFilter,
        IAsyncServerFilter,
        IOrderedFilter,
        IServerResultFilter,
        IAsyncServerResultFilter,
        IServerFilterMetadata
    {
        
        public int Order { get; set; }

        public virtual void OnActionExecuting(ServerInvokeExecutingContext context)
        {
        }

        public virtual void OnActionExecuted(ServerInvokeExecutedContext context)
        {
        }

        public virtual async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
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

        public virtual void OnResultExecuting(ServerResultExecutingContext context)
        {
           
        }

        public virtual void OnResultExecuted(ServerResultExecutedContext context)
        {
            
        }

        public virtual async Task OnResultExecutionAsync(ServerResultExecutingContext context, ServerResultExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof (context));
            if (next == null)
                throw new ArgumentNullException(nameof (next));
            this.OnResultExecuting(context);
            if (context.Cancel)
                return;
            this.OnResultExecuted(await next());
        }
    }
}