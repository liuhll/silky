using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Silky.Core.Extensions;
using Silky.Core.Serialization;

namespace Silky.Core.Runtime.Rpc
{
    public class RpcContext
    {
        private ConcurrentDictionary<string, string> invokeAttachments;
        private ConcurrentDictionary<string, string> resultAttachments;
        private ConcurrentDictionary<string, string> transAttachments;
        private static AsyncLocal<RpcContext> rpcContextThreadLocal = new();

        private RpcContext()
        {
            invokeAttachments = new(StringComparer.OrdinalIgnoreCase);
            resultAttachments = new(StringComparer.OrdinalIgnoreCase);
            transAttachments = new(StringComparer.OrdinalIgnoreCase);
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

        public void RemoveInvokeAttachment([NotNull] string key)
        {
            invokeAttachments.TryRemove(key, out _);
        }

        public void RemoveResultAttachment([NotNull] string key)
        {
            resultAttachments.TryRemove(key, out _);
        }

        public void RemoveTransAttachment([NotNull] string key)
        {
            transAttachments.TryRemove(key, out _);
        }

        public void SetInvokeAttachment([NotNull] string key, string value)
        {
            invokeAttachments.AddOrUpdate(key, value, (k, v) => value);
        }

        public void SetInvokeAttachment([NotNull] string key, object value)
        {
            if (value is string stringValue)
            {
                invokeAttachments.AddOrUpdate(key, stringValue, (k, v) => stringValue);
            }
            else
            {
                var serializer = EngineContext.Current.Resolve<ISerializer>();
                var jsonValue = serializer.Serialize(value);
                invokeAttachments.AddOrUpdate(key, jsonValue, (k, v) => jsonValue);
            }
        }


        public void SetTransAttachment([NotNull] string key, object value)
        {
            if (value is string stringValue)
            {
                transAttachments.AddOrUpdate(key, stringValue, (k, v) => stringValue);
            }
            else
            {
                var serializer = EngineContext.Current.Resolve<ISerializer>();
                var jsonValue = serializer.Serialize(value);
                transAttachments.AddOrUpdate(key, jsonValue, (k, v) => jsonValue);
            }
        }

        public void SetResultAttachment([NotNull] string key, object value)
        {
            if (value is string stringValue)
            {
                resultAttachments.AddOrUpdate(key, stringValue, (k, v) => stringValue);
            }
            else
            {
                var serializer = EngineContext.Current.Resolve<ISerializer>();
                var jsonValue = serializer.Serialize(value);
                resultAttachments.AddOrUpdate(key, jsonValue, (k, v) => jsonValue);
            }
        }

        public bool HasInvokeAttachment([NotNull] string key)
        {
            return invokeAttachments.ContainsKey(key);
        }

        public bool HasResultAttachment([NotNull] string key)
        {
            return resultAttachments.ContainsKey(key);
        }

        public bool HasTransAttachment([NotNull] string key)
        {
            return transAttachments.ContainsKey(key);
        }

        public string? GetInvokeAttachment([NotNull] string key)
        {
            invokeAttachments.TryGetValue(key, out string result);
            return result;
        }

        public string? GetResultAttachment([NotNull] string key)
        {
            resultAttachments.TryGetValue(key, out string? result);
            return result;
        }

        public string? GetTransAttachment([NotNull] string key)
        {
            transAttachments.TryGetValue(key, out string result);
            return result;
        }


        public bool TryGetInvokeAttachment([NotNull] string key, out string? result)
        {
            return invokeAttachments.TryGetValue(key, out result);
        }

        public bool TryGetResultAttachment([NotNull] string key, out string? result)
        {
            return resultAttachments.TryGetValue(key, out result);
        }

        public bool TryGetTransAttachment([NotNull] string key, out string? result)
        {
            return transAttachments.TryGetValue(key, out result);
        }

        public void SetInvokeAttachments(IDictionary<string, string> attachments)
        {
            foreach (var item in attachments)
            {
                SetInvokeAttachment(item.Key, item.Value);
            }
        }

        public void SetTransAttachments(IDictionary<string, string> attachments)
        {
            foreach (var item in attachments)
            {
                SetTransAttachment(item.Key, item.Value);
            }
        }

        public void SetResponseHeader([NotNull] string key, string value)
        {
            IDictionary<string, string> responseHeader;
            if (AttachmentKeys.ResponseHeader.Equals(key))
            {
                var responseHeaderValue = GetResultAttachment(AttachmentKeys.ResponseHeader);
                responseHeader = responseHeaderValue.ConventTo<IDictionary<string, string>>();
            }
            else
            {
                responseHeader = new Dictionary<string, string>();
            }

            responseHeader[key] = value;
            SetResultAttachment(AttachmentKeys.ResponseHeader, responseHeader);
        }


        public void SetResultAttachments(IDictionary<string, string> attachments)
        {
            if (attachments == null)
            {
                return;
            }

            foreach (var item in attachments)
            {
                SetResultAttachment(item.Key, item.Value);
            }
        }

        public IDictionary<string, string> GetInvokeAttachments()
        {
            return invokeAttachments;
        }

        public IDictionary<string, string> GetResultAttachments()
        {
            return resultAttachments;
        }

        public IDictionary<string, string> GetTransAttachments()
        {
            return transAttachments;
        }

        public IDictionary<string, string> GetResponseHeaders()
        {
            var responseHeadersValue = GetResultAttachment(AttachmentKeys.ResponseHeader);
            if (responseHeadersValue == null)
            {
                return null;
            }

            return responseHeadersValue.ConventTo<IDictionary<string, string>>();
        }
    }
}