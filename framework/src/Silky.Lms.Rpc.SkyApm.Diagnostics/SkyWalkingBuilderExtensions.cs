using System;
using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace Silky.Lms.Rpc.SkyApm.Diagnostics
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddRpc(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, RpcTracingDiagnosticProcessor>();
            
            return extensions;
        }
    }
}