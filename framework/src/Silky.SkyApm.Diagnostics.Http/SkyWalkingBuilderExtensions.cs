using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using Silky.SkyApm.Diagnostics.Rpc.Http;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace Silky.SkyApm.Diagnostics.Http
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddSkyApmSilkyHttp(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, HttpTracingDiagnosticProcessor>();
            extensions.Services.TryAdd(ServiceDescriptor
                .Singleton<ISilkySegmentContextFactory, SilkySegmentContextFactory>());
            return extensions;
        }
    }
}