using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server
{
    public interface IServiceIdGenerator : ISingletonDependency
    {
        string GenerateServiceId([NotNull]MethodInfo method);
    }
}