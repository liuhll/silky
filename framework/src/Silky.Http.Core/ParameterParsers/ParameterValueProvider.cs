using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public abstract class ParameterValueProvider
{
    protected ServiceEntry _serviceEntry;

    public ParameterValueProvider(ServiceEntry serviceEntry)
    {
        _serviceEntry = serviceEntry;
    }

    protected IDictionary<string, Type> GetKeysFromParameter(ParameterFrom @from)
    {
        var keys = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var @params = _serviceEntry.ParameterDescriptors.Where(p => p.From == from);
        foreach (var param in @params)
        {
            if (param.IsSampleOrNullableType)
            {
                keys.Add(param.Name, param.Type);
            }
            else
            {
                var properties = param.Type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    keys.Add(propertyInfo.Name, propertyInfo.PropertyType);
                }
            }
        }

        return keys;
    }

    protected bool IsEnumerable(Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }

        var isEnumerableOfT = type.GetInterfaces()
            .Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return isEnumerableOfT;
    }
}