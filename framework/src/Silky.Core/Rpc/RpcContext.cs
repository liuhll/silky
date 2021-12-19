using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Rpc
{
    public class RpcContext
    {
        private ConcurrentDictionary<string, object> invokeAttachments;
        private ConcurrentDictionary<string, object> resultAttachments;
        private static AsyncLocal<RpcContext> rpcContextThreadLocal = new();

        private RpcContext()
        {
            invokeAttachments = new(StringComparer.OrdinalIgnoreCase);
            resultAttachments = new(StringComparer.OrdinalIgnoreCase);
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

        public void SetInvokeAttachment([NotNull] string key, object value)
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

            invokeAttachments.AddOrUpdate(key, value, (k, v) => value);
        }

        public void SetResultAttachment([NotNull] string key, object value)
        {
            if (AttachmentKeys.ResponseHeader.Equals(key))
            {
                var convertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                value = convertibleService.Convert(value, typeof(IDictionary<string, object>));
            }

            if (value?.GetType().GetObjectDataType() == ObjectDataType.Complex
                && !(value is IDictionary<string, object>)
                && !(value is IDictionary<string, string>)
                && !(value is IList<string>)
                && !(value is JObject)
                && !(value is JArray)
               )
            {
                throw new SilkyException("rpcContext attachments complex type parameters are not allowed");
            }

            if (value is IDictionary<string, object> dict)
            {
                var resultAttachment = GetResultAttachment(key);
                if (resultAttachment != null)
                {
                    var resultAttachmentValue = resultAttachment.ConventTo<IDictionary<string, object>>();
                    foreach (var item in resultAttachmentValue)
                    {
                        if (!dict.ContainsKey(item.Key))
                        {
                            dict[item.Key] = item.Value;
                        }
                    }
                }
            }

            if (value is IDictionary<string, string> dict1)
            {
                var resultAttachment = GetResultAttachment(key);
                if (resultAttachment != null)
                {
                    var resultAttachmentValue = resultAttachment.ConventTo<IDictionary<string, string>>();
                    foreach (var item in resultAttachmentValue)
                    {
                        if (!dict1.ContainsKey(item.Key))
                        {
                            dict1[item.Key] = item.Value;
                        }
                    }
                }
            }

            if (value is List<string> list)
            {
                var resultAttachment = GetResultAttachment(key);
                if (resultAttachment != null)
                {
                    var resultAttachmentValue = resultAttachment.ConventTo<List<string>>();
                    foreach (var item in resultAttachmentValue)
                    {
                        list.AddIfNotContains(item);
                    }
                }
            }

            resultAttachments.AddOrUpdate(key, value, (k, v) => value);
        }

        public bool HasInvokeAttachment([NotNull] string key)
        {
            return invokeAttachments.ContainsKey(key);
        }

        public bool HasResultAttachment([NotNull] string key)
        {
            return resultAttachments.ContainsKey(key);
        }

        public object GetInvokeAttachment([NotNull] string key)
        {
            invokeAttachments.TryGetValue(key, out object result);
            return result;
        }

        public object GetResultAttachment([NotNull] string key)
        {
            resultAttachments.TryGetValue(key, out object result);
            return result;
        }

        public void SetInvokeAttachments(IDictionary<string, object> attachments)
        {
            foreach (var item in attachments)
            {
                SetInvokeAttachment(item.Key, item.Value);
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


        public void SetResultAttachments(IDictionary<string, object> attachments)
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

        public IDictionary<string, object> GetInvokeAttachments()
        {
            return invokeAttachments;
        }

        public IDictionary<string, object> GetResultAttachments()
        {
            return resultAttachments;
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