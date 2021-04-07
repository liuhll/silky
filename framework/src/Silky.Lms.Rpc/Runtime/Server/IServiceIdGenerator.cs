using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Runtime.Server
{
    public interface IServiceIdGenerator : ISingletonDependency
    {
        string GenerateServiceId([NotNull]MethodInfo method);
    }
}