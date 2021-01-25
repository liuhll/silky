using System.Reflection;
using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lms.Rpc.Ids
{
    public interface IServiceIdGenerator : ITransientDependency
    {
        string GenerateServiceId(MethodInfo method, HttpMethodAttribute httpMethod);
    }
}