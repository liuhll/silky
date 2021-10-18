using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;

namespace Silky.Core.Rpc
{
    public class RpcContext
    {
        private ConcurrentDictionary<string, object> contextAttachments;
        private static AsyncLocal<RpcContext> rpcContextThreadLocal = new();

        private RpcContext()
        {
            contextAttachments = new(StringComparer.OrdinalIgnoreCase);
        }

        public static RpcContext Context
        {
            get
            {
                var context = rpcContextThreadLocal.Value;
                if (context == null)
                {
                    context = new RpcContext();
                    rpcContextThreadLocal.Value = context;
                }

                return rpcContextThreadLocal.Value;
            }
        }

        public RpcConnection Connection => GetRpcConnection();

        private RpcConnection GetRpcConnection()
        {
            var clientHost = this.GetClientHost();
            var clientProtocol = this.GetClientServiceProtocol();
            var clientPort = this.GetClientPort();
            var remotePort = this.GetRpcRequestPort();
            var localHost = this.GetLocalHost();
            var localProtocol = this.GetLocalServiceProtocol();
            var localPort = this.GetLocalPort();
            var rpcConnection = new RpcConnection()
            {
                ClientHost = clientHost,
                RemotePort = remotePort,
                ClientServiceProtocol = clientProtocol,
                ClientPort = clientPort,
                LocalHost = localHost,
                LocalPort = localPort,
                LocalServiceProtocol = localProtocol
            };
            return rpcConnection;
        }

        public IServiceProvider RpcServices { set; get; }

        public void RemoveAttachment(string key)
        {
            contextAttachments.TryRemove(key, out _);
        }

        public void SetAttachment(string key, object value)
        {
            if (AttachmentKeys.RequestHeader.Equals(key))
            {
                var convertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                value = convertibleService.Convert(value, typeof(IDictionary<string, object>));
            }

            if (value?.GetType().GetObjectDataType() == ObjectDataType.Complex
                && !(value is IDictionary<string, object>)
                && !(value is JObject)
                && !(value is JArray)
            )
            {
                throw new SilkyException("rpcContext attachments complex type parameters are not allowed");
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