using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Routing
{
    public interface IRouter
    {
        RouteTemplate RouteTemplate { get; }
        
        public HttpMethod HttpMethod { get; }

        public string RoutePath { get; }

        bool IsMatch(string api,HttpMethod httpMethod);

        IDictionary<string,object> ParserRouteParameters(string path);
    }
}