using System.Collections.Generic;
using System.Reflection;
using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IHttpMethodProvider : ITransientDependency
    {
        (IReadOnlyList<HttpMethodAttribute>,bool) GetHttpMethodsInfo(MethodInfo method);
    }
}