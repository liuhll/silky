using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IIdGenerator : ITransientDependency
    {
        string GenerateServiceEntryId([NotNull] MethodInfo method, HttpMethod httpMethod);

        string GetDefaultServiceEntryId([NotNull] MethodInfo method);

        string GenerateServiceId([NotNull] Type serviceType);
    }
}