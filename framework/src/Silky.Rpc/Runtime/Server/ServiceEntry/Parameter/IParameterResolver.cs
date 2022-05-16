using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server;

public interface IParameterResolver
{
    object[] Parser(ServiceEntry serviceEntry, RemoteInvokeMessage message);

    object[] Resolve(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters);

    object[] Resolve(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters, HttpContext httpContext);
}