using System;
using System.Threading.Tasks;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Class | System.AttributeTargets.Interface | System.AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public abstract class FilterAttribute :
        Attribute,
        IServerFilter,
        IAsyncServerFilter,
        IOrderedFilter,
        IServerResultFilter,
        IAsyncServerResultFilter,
        IServerFilterMetadata
    {
        
        public int Order { get; set; }

        public virtual void OnActionExecuting(ServerExecutingContext context)
        {
        }

        public virtual void OnActionExecuted(ServerExecutedContext context)
        {
        }

        public virtual async Task OnActionExecutionAsync(ServerExecutingContext context, ServerExecutionDelegate next)
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

        public virtual async Task OnResultExecutionAsync(ServerResultExecutingContext context, ResultExecutionDelegate next)
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