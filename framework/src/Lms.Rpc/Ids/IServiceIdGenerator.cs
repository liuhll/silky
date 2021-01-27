using System.Reflection;
using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Ids
{
    public interface IServiceIdGenerator : ITransientDependency
    {
        string GenerateServiceId(MethodInfo method, HttpMethod httpMethod);
    }
}