using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Routing.Template;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class HttpParameterResolver : ParameterResolverBase
{
    private readonly ISerializer _serializer;

    public HttpParameterResolver(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public override object[] Parser([NotNull] ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        Check.NotNull(serviceEntry, nameof(serviceEntry));
        if (serviceEntry.ParameterDescriptors.Any(p => p.From == ParameterFrom.Path) &&
            message.HttpParameters.All(p => p.Key != ParameterFrom.Path))
        {
            var path = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.Path);
            var pathData = serviceEntry.Router.ParserRouteParameters(path);
            message.HttpParameters.Add(ParameterFrom.Path, _serializer.Serialize(pathData));
        }

        var parameters = Parser(serviceEntry, message.HttpParameters, null);
        parameters = serviceEntry.ConvertParameters(parameters);
        return parameters;
    }

    public override object[] Parser(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters,
        HttpContext httpContext)
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
                    else if (httpContext != null && parameterDescriptor.IsMultipleFileParameter())
                    {
                        list.Add(httpContext.Request.Form.Files);
                    }
                    else if (httpContext != null && parameterDescriptor.IsSingleFileParameter())
                    {
                        list.Add(httpContext.Request.Form.Files.GetFile(parameterDescriptor.Name));
                    }
                    else if (httpContext != null)
                    {
                        list.Add(parameterDescriptor.GetActualParameter(parameter, httpContext));
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
                        if (!pathVal.TryOrdinalIgnoreCaseGetValue(parameterName, out var parameterVal))
                        {
                            throw new SilkyException(
                                "The path parameter is not allowed to be empty, please confirm whether the parameter you passed is correct");
                        }

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
}