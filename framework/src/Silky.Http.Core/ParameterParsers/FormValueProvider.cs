using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public class FormValueProvider : ServiceEntryValueProvider
{
    private readonly IFormCollection _values;
    private PrefixContainer _prefixContainer;

    public FormValueProvider(ServiceEntry serviceEntry, IFormCollection value) : base(serviceEntry)
    {
        _values = value;
    }


    public IDictionary<string, object> GetFormData()
    {
        var formParamKeys = GetKeysFromFrom(ParameterFrom.Form);
        var formData = new Dictionary<string, object>();

        foreach (var value in _values)
        {
            _ = formParamKeys.TryGetValue(value.Key, out var type);
            if (type?.IsArray == true || IsEnumerable(type))
            {
                formData[value.Key] = value.Value.ToString().Split(",");
            }
            else
            {
                formData[value.Key] = value.Value.ToString();
            }
        }

        return formData;
    }
}