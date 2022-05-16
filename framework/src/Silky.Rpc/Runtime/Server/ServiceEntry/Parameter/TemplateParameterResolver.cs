using System.Collections.Generic;
using Silky.Core.Convertible;
using Silky.Core.MethodExecutor;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class TemplateParameterResolver : ParameterResolverBase
{
    private readonly ITypeConvertibleService _typeConvertibleService;

    public TemplateParameterResolver(ITypeConvertibleService typeConvertibleService)
    {
        _typeConvertibleService = typeConvertibleService;
    }

    public override object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        var parameters = new List<object>();
        foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
        {
            if (message.DictParameters.TryGetValue(parameterDescriptor.Name, out var parameterVal))
            {
                var parameter = _typeConvertibleService.Convert(parameterVal, parameterDescriptor.Type);
                parameters.Add(parameter);
            }
            else
            {
                var defaultParameterVal = parameterDescriptor.ParameterInfo.GetDefaultValue();
                parameters.Add(defaultParameterVal);
            }
        }

        return parameters.ToArray();
    }
}