using System;
using System.Collections.Generic;
using System.Linq;

namespace Silky.Rpc.CachingInterceptor;

public static class DictionaryExtensions
{
    public static bool TryOrdinalIgnoreCaseGetValue(this IDictionary<string, object> dictionary, string propName, out object value)
    {
        
        if (dictionary.Keys.Any(key => key.Equals(propName, StringComparison.OrdinalIgnoreCase)))
        {
            var key = dictionary.Keys.First(key => key.Equals(propName, StringComparison.OrdinalIgnoreCase));
            value = dictionary[key];
            return true;
        }
        value = null;
        return false;
    }
}