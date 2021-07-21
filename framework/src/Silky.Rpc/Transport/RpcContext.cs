using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;

namespace Silky.Rpc.Transport
{
    public class RpcContext
    {
        private ConcurrentDictionary<string, object> contextAttachments;
        private static AsyncLocal<RpcContext> rpcContextThreadLocal = new();

        private RpcContext()
        {
            contextAttachments = new();
        }

        public static RpcContext GetContext()
        {
            var context = rpcContextThreadLocal.Value;
            if (context == null)
            {
                context = new RpcContext();
                rpcContextThreadLocal.Value = context;
            }

            return rpcContextThreadLocal.Value;
        }

        public void SetAttachment(string key, object value)
        {
            if (AttachmentKeys.RequestHeader.Equals(key))
            {
                var convertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                value = convertibleService.Convert(value, typeof(IDictionary<string, object>));
            }

            if (value.GetType().GetObjectDataType() == ObjectDataType.Complex &&
                !(value is IDictionary<string,object>) && 
                value.GetType().FullName != "Silky.Transaction.TransactionContext")
            {
                throw new SilkyException("rpcContext attachments 不允许设置复杂类型参数");
            }

            contextAttachments.AddOrUpdate(key, value, (k, v) => value);
        }

        public bool HasAttachment(string key)
        {
            return contextAttachments.ContainsKey(key);
        }

        public object GetAttachment(string key)
        {
            contextAttachments.TryGetValue(key, out object result);
            return result;
        }

        public void SetAttachments(IDictionary<string, object> attachments)
        {
            foreach (var item in attachments)
            {
                SetAttachment(item.Key, item.Value);
            }
        }

        public IDictionary<string, object> GetContextAttachments()
        {
            return contextAttachments;
        }
    }
}