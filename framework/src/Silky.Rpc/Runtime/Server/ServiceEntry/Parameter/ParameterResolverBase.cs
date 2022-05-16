using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.MethodExecutor;
using Silky.Rpc.Routing.Template;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public abstract class ParameterResolverBase : IParameterResolver
{
    public abstract object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message);


    public virtual object[] Resolve(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters)
    {
        var list = new List<object>();
        var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
        {
            #region 获取参数

            var parameter = parameterDescriptor.From.DefaultValue();
            if (parameters.ContainsKey(parameterDescriptor.From))
            {
                parameter = parameters[parameterDescriptor.From];
            }

            switch (parameterDescriptor.From)
            {
                case ParameterFrom.Body:
                    list.Add(parameterDescriptor.GetActualParameter(parameter));
                    break;
                case ParameterFrom.Form:
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                    }
                    else
                    {
                        list.Add(parameterDescriptor.GetActualParameter(parameter));
                    }

                    break;
                case ParameterFrom.Header:
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                    }
                    else
                    {
                        list.Add(parameterDescriptor.GetActualParameter(parameter));
                    }

                    break;
                case ParameterFrom.Path:
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        var pathVal =
                            (IDictionary<string, object>)typeConvertibleService.Convert(parameter,
                                typeof(IDictionary<string, object>));
                        var parameterName = TemplateSegmentHelper.GetVariableName(parameterDescriptor.Name);
                        if (!pathVal.ContainsKey(parameterName))
                        {
                            throw new SilkyException(
                                "The path parameter is not allowed to be empty, please confirm whether the parameter you passed is correct");
                        }

                        var parameterVal = pathVal[parameterName];
                        list.Add(parameterDescriptor.GetActualParameter(parameterVal));
                    }
                    else
                    {
                        throw new SilkyException(
                            "Complex data types do not support access through routing templates");
                    }

                    break;
                case ParameterFrom.Query:
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                    }
                    else
                    {
                        list.Add(parameterDescriptor.GetActualParameter(parameter));
                    }

                    break;
            }

            #endregion
        }

        return list.ToArray();
    }
    

    public virtual object[] Resolve(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters,
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

        list.Add(parameterDescriptor.GetActualParameter(parameterVal));
    }
}