using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public class HttpParameterResolver : IParameterResolver
{
    private readonly ISerializer _serializer;

    public HttpParameterResolver(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public object[] Parser([NotNull] ServiceEntry serviceEntry, RemoteInvokeMessage message)
    {
        Check.NotNull(serviceEntry, nameof(serviceEntry));
        if (serviceEntry.ParameterDescriptors.Any(p => p.From == ParameterFrom.Path))
        {
            var path = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.Path).ToString();
            var pathData = serviceEntry.Router.ParserRouteParameters(path);
            message.HttpParameters.Add(ParameterFrom.Path, _serializer.Serialize(pathData));
        }

        var parameters = serviceEntry.ResolveParameters(message.HttpParameters);
        parameters = serviceEntry.ConvertParameters(parameters);
        return parameters;
    }
}