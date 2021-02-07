using System.Collections.Generic;
using Lms.Rpc.Routing.Template;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Routing
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