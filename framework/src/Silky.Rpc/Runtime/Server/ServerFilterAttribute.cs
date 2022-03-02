using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Class | System.AttributeTargets.Interface | System.AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public abstract class ServerFilterAttribute : Attribute, IServerFilter
    {
        protected ServerFilterAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public virtual void OnActionExecuting(ServerExecutingContext context)
        {
        }

        public virtual void OnActionExecuted(ServerExecutedContext context)
        {
        }

        public virtual void OnActionException(ServerExceptionContext context)
        {
        }
    }
}