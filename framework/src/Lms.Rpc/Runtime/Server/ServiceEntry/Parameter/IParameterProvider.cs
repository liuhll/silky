using System.Collections.Generic;
using System.Reflection;
using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public interface IParameterProvider : ITransientDependency
    {
        IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethod httpMethod);
    }
}