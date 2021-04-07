using System.Collections.Generic;
using System.Reflection;
using Silky.Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IHttpMethodProvider : ITransientDependency
    {
        (IReadOnlyList<HttpMethodAttribute>,bool) GetHttpMethodsInfo(MethodInfo method);
    }
}