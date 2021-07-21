using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceIdGenerator : ISingletonDependency
    {
        string GenerateServiceId([NotNull]MethodInfo method);
    }
}