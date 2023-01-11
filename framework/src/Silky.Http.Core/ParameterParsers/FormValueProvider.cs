using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public class FormValueProvider : ParameterValueProvider
{
    private readonly IFormCollection _values;
    private PrefixContainer _prefixContainer;
    private readonly ISerializer _serializer;

    public FormValueProvider(ServiceEntry serviceEntry, ISerializer serializer, IFormCollection value) : base(serviceEntry)
    {
        _values = value;
        _serializer = serializer;
    }


    public IDictionary<string, object> GetFormData()
    {
        var formParamKeys = GetKeysFromParameter(ParameterFrom.Form);
        var formData = new Dictionary<string, object>();

        foreach (var value in _values)
        {

            var hasParamKey = formParamKeys.TryGetValue(value.Key, out var type);
            if (hasParamKey)
            {
                if (type.IsEnumerable())
                {
                    formData[value.Key] =_serializer.Deserialize(type, $"[{value.Value.ToString()}]");
                }
                else
                {
                    formData[value.Key] = value.Value.ToString();
                }
            }
            else
            {
                formData[value.Key] = value.Value.ToString();
            }
        }

        return formData;
    }
}