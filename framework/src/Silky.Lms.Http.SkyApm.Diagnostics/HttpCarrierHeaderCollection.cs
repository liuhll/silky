using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SkyApm.Tracing;

namespace Silky.Lms.Http.SkyApm.Diagnostics
{
    public class HttpCarrierHeaderCollection : ICarrierHeaderDictionary
    {
        private readonly IHeaderDictionary _headers;

        public HttpCarrierHeaderCollection(HttpRequest httpRequest)
        {
            _headers = httpRequest.Headers;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Add(string key, string value)
        {
            throw new System.NotImplementedException();
        }

        public string Get(string key)
        {
            if (_headers.TryGetValue(key, out var value))
                return value;
            return null;
        }
    }
}