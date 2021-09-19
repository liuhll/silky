using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;
using Silky.Core.Rpc;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceKeyExecutor : IServiceKeyExecutor, IScopedDependency
    {
        public string ServiceKey => RpcContext.Context.GetAttachment(AttachmentKeys.ServiceKey)?.ToString();


        public async Task Execute(Func<Task> func, string serviceKey)
        {
            var currentServiceKey = SetServiceKeyBeforeInvoke(serviceKey);
            await func.Invoke();
            SetServiceKeyAfterInvoke(currentServiceKey);
        }

        public async Task<T> Execute<T>(Func<Task<T>> func, string serviceKey)
        {
            var currentServiceKey = SetServiceKeyBeforeInvoke(serviceKey);
            var result = await func.Invoke();
            SetServiceKeyAfterInvoke(currentServiceKey);
            return result;
        }


        private string SetServiceKeyBeforeInvoke(string serviceKey)
        {
            var currentServiceKey = ServiceKey;
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetAttachment(AttachmentKeys.ServiceKey, serviceKey);
            }

            return currentServiceKey;
        }

        private void SetServiceKeyAfterInvoke(string currentServiceKey)
        {
            if (!currentServiceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetAttachment(AttachmentKeys.ServiceKey, currentServiceKey);
            }
            else
            {
                RpcContext.Context.RemoveAttachment(AttachmentKeys.ServiceKey);
            }
        }
    }
}