using System.Collections.Generic;
using System.Reflection;
using Silky.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Rpc.Runtime.Server
{
    public interface IParameterProvider : ITransientDependency
    {
        IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethod httpMethod);
    }
}