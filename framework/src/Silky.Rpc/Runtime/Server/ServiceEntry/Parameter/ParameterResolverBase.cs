using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Core.Convertible;
using Silky.Core.MethodExecutor;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public abstract class ParameterResolverBase : IParameterResolver
{
    public abstract object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message);


    public virtual object[] Parser(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters,
        HttpContext httpContext)
    {
        throw new System.NotImplementedException();
    }

    protected void SetSampleParameterValue(ITypeConvertibleService typeConvertibleService, object parameter,
        ParameterDescriptor parameterDescriptor, List<object> list)
    {
        var dict =
            (IDictionary<string, object>)typeConvertibleService.Convert(parameter,
                typeof(IDictionary<string, object>));
        var parameterVal = parameterDescriptor.ParameterInfo.GetDefaultValue();
        if (dict.ContainsKey(parameterDescriptor.Name))
        {
            parameterVal = dict[parameterDescriptor.Name];
        }

        list.Add(parameterDescriptor.GetActualParameter(parameterVal) ??
                 parameterDescriptor.ParameterInfo.GetDefaultValue());
    }
}