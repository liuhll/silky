using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Lms.Core.MethodExecutor;
using Lms.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public class HttpMethodProvider : IHttpMethodProvider
    {
        
        public (IReadOnlyList<HttpMethodAttribute>,bool) GetHttpMethodsInfo(MethodInfo method)
        {
            var httpMethods = method.GetCustomAttributes().OfType<HttpMethodAttribute>();
            if (httpMethods.Any())
            {
                return (httpMethods.ToImmutableList(),true);
            }

            if (method.GetParameters().All(p=> p.IsSampleType()))
            {
                return (new[] {new HttpGetAttribute()},false);
            }
            return (new[] {new HttpPostAttribute()},false);
        }
    }
}