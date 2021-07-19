using System;
using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace Silky.Lms.Http.SkyApm.Diagnostics
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddSilkyHttpServer(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, HttpTracingDiagnosticProcessor>();
            return extensions;
        }
    }
}