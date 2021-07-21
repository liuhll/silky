using System.Collections.Generic;
using System.Reflection;
using Silky.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IHttpMethodProvider : ITransientDependency
    {
        (IReadOnlyList<HttpMethodAttribute>,bool) GetHttpMethodsInfo(MethodInfo method);
    }
}