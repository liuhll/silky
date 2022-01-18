using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public class QueryStringValueProvider : ServiceEntryValueProvider
{
    private readonly IQueryCollection _values;
    private PrefixContainer _prefixContainer;

    public QueryStringValueProvider(ServiceEntry serviceEntry, IQueryCollection values) : base(serviceEntry)
    {
        _values = values;
    }

    public IDictionary<string, object> GetQueryData()
    {
        var queryParamKeys = GetKeysFromFrom(ParameterFrom.Query);
        var queryData = new Dictionary<string, object>();

        foreach (var value in _values)
        {
            _ = queryParamKeys.TryGetValue(value.Key, out var type);
            if (type?.IsArray == true || IsEnumerable(type))
            {
                queryData[value.Key] = value.Value.ToString().Split(",");
            }
            else
            {
                queryData[value.Key] = value.Value.ToString();
            }
        }

        return queryData;
    }
    
}