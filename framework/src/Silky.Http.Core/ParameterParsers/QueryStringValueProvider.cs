using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public class QueryStringValueProvider : ParameterValueProvider
{
    private readonly IQueryCollection _values;
    private PrefixContainer _prefixContainer;
    private readonly ISerializer _serializer;

    public QueryStringValueProvider(ServiceEntry serviceEntry, ISerializer serializer, IQueryCollection values) :
        base(serviceEntry)
    {
        _values = values;
        _serializer = serializer;
    }

    public IDictionary<string, object> GetQueryData()
    {
        var queryParamKeys = GetKeysFromParameter(ParameterFrom.Query);
        var queryData = new Dictionary<string, object>();

        foreach (var value in _values)
        {
            var hasParamKey = queryParamKeys.TryGetValue(value.Key, out var type);
            if (hasParamKey)
            {
                if (type.IsEnumerable())
                {
                    queryData[value.Key] = _serializer.Deserialize(type, $"[{value.Value.ToString()}]");
                }
                else
                {
                    queryData[value.Key] = value.Value.ToString();
                }
            }
            else
            {
                queryData[value.Key] = value.Value.ToString();
            }
        }

        return queryData;
    }
}