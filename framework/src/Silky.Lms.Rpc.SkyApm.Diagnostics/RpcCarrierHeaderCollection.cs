using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Silky.Lms.Core;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Messages;
using SkyApm.Tracing;

namespace Silky.Lms.Rpc.SkyApm.Diagnostics
{
    public class RpcCarrierHeaderCollection : ICarrierHeaderDictionary
    {
        private readonly RemoteInvokeMessage _message;
        private readonly ISerializer _serializer;

        public RpcCarrierHeaderCollection(RemoteInvokeMessage message)
        {
            _message = message;
            _serializer = EngineContext.Current.Resolve<ISerializer>();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _message.Attachments
                .Select(x => new KeyValuePair<string, string>(x.Key, _serializer.Serialize(x.Value))).GetEnumerator();
        }

        public void Add(string key, string value)
        {
            if (_message.Attachments.ContainsKey(key))
            {
                _message.Attachments.Remove(key);
            }

            _message.Attachments.Add(key, value);
        }

        public string Get(string key)
        {
            if (_message.Attachments.TryGetValue(key, out var value))
                return _serializer.Serialize(value);
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}