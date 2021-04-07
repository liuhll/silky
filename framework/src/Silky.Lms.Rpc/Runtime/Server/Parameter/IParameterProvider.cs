using System.Collections.Generic;
using System.Reflection;
using Silky.Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Lms.Rpc.Runtime.Server.Parameter
{
    public interface IParameterProvider : ITransientDependency
    {
        IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethod httpMethod);
    }
}