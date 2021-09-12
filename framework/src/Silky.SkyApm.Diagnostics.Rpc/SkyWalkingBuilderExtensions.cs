using System;
using Microsoft.Extensions.DependencyInjection;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using Silky.SkyApm.Diagnostics.Rpc.Client;
using Silky.SkyApm.Diagnostics.Rpc.Http;
using Silky.SkyApm.Diagnostics.Rpc.Server;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace Silky.SkyApm.Diagnostics.Rpc
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddSilkyRpc(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, RpcClientTracingDiagnosticProcessor>();
            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, RpcServerTracingDiagnosticProcessor>();
            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, HttpTracingDiagnosticProcessor>();
            extensions.Services.AddSingleton<ISilkySegmentContextFactory, SilkySegmentContextFactory>();
            return extensions;
        }
    }
}