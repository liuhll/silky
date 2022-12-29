using System.Collections.Generic;
using System.Reflection;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IParameterProvider : ITransientDependency
    {
        IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethodInfo httpMethodInfo);
    }
}