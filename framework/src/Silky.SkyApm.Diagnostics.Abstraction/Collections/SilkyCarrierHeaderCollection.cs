using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Rpc;
using SkyApm.Tracing;

namespace Silky.SkyApm.Diagnostics.Abstraction.Collections
{
    public class SilkyCarrierHeaderCollection : ICarrierHeaderDictionary
    {
        private readonly RpcContext _rpcContext;

        public SilkyCarrierHeaderCollection(RpcContext rpcContext)
        {
            _rpcContext = rpcContext;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _rpcContext.GetContextAttachments()
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())).GetEnumerator();
        }

        public void Add(string key, string value)
        {
            if (_rpcContext.GetContextAttachments().ContainsKey(key))
            {
                _rpcContext.GetContextAttachments().Remove(key);
            }

            _rpcContext.GetContextAttachments().Add(key, value);
        }

        public string Get(string key)
        {
            if (_rpcContext.GetContextAttachments().TryGetValue(key, out var value))
                return value.ToString();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}