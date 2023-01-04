using System;
using System.Threading.Tasks;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Class | System.AttributeTargets.Interface | System.AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public abstract class ServerFilterAttribute :
        Attribute,
        IServerFilter,
        IAsyncServerServerFilter,
        IOrderedServerFilter,
        IResultServerFilter,
        IAsyncResultFilter,
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

        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
           
        }

        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
            
        }

        public virtual async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
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