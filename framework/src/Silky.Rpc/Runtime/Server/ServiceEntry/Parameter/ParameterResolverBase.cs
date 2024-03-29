﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Core.Convertible;
using Silky.Core.Extensions.Collections.Generic;
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
        RpcParameter rpcParameter, List<object> list)
    {
        var dict =
            (IDictionary<string, object>)typeConvertibleService.Convert(parameter,
                typeof(IDictionary<string, object>));
        var parameterVal = rpcParameter.ParameterInfo.GetDefaultValue();
        if (dict.TryOrdinalIgnoreCaseGetValue(rpcParameter.Name,out var dictValue))
        {
            parameterVal = dictValue;
        }

        list.Add(rpcParameter.GetActualParameter(parameterVal) ??
                 rpcParameter.ParameterInfo.GetDefaultValue());
    }
}