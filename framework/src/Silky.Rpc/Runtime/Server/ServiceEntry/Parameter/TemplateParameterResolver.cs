using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.MethodExecutor;
using Silky.Core.Serialization;
using Silky.Rpc.Extensions;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class TemplateParameterResolver : ParameterResolverBase
{
    private readonly ITypeConvertibleService _typeConvertibleService;
    private readonly ISerializer _serializer;

    public TemplateParameterResolver(ITypeConvertibleService typeConvertibleService,
        ISerializer serializer)
    {
        _typeConvertibleService = typeConvertibleService;
        _serializer = serializer;
    }

    public override object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        var parameters = new List<object>();
        foreach (var parameterDescriptor in serviceEntry.Parameters)
        {
            if (message.DictParameters.TryGetValue(parameterDescriptor.Name, out var parameterVal))
            {
                if (parameterDescriptor.IsSingleFileParameter())
                {
                    var silkyFormFile = _serializer.Deserialize<SilkyFormFile>(parameterVal.ToString());
                    var formFileParameter = silkyFormFile.ConventToFormFile();
                    parameters.Add(formFileParameter);
                    continue;
                }

                if (parameterDescriptor.IsMultipleFileParameter())
                {
                    var silkyFormFiles = _serializer.Deserialize<SilkyFormFile[]>(parameterVal.ToString());
                    var formFileCollectionParameter = silkyFormFiles.ConventToFileCollection();
                    parameters.Add(formFileCollectionParameter);
                    continue;
                }
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