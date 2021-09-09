using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceIdGenerator : ITransientDependency
    {
        string GenerateServiceEntryId([NotNull]MethodInfo method);

        string GenerateServiceId([NotNull] Type serviceType);
    }
}