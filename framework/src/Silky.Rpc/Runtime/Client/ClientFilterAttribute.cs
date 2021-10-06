using System;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    [AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method, AllowMultiple = true,
        Inherited = true)]
    public abstract class ClientFilterAttribute : Attribute, IClientFilter
    {
        protected ClientFilterAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public virtual void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage)
        {
        }

        public virtual void OnActionExecuted(RemoteResultMessage context)
        {
        }
    }
}