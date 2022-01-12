using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public static class ServiceEntryExtensions
{
    public static object[] GetParameters(this ServiceEntry serviceEntry, HttpRequest httpRequest)
    {
        var parameterParser = EngineContext.Current.Resolve<IParameterParser>();
        return parameterParser.Parser(serviceEntry, httpRequest);
    }
}