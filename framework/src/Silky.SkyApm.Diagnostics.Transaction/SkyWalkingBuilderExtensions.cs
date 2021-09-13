using System;
using Microsoft.Extensions.DependencyInjection;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using Silky.SkyApm.Diagnostics.Transaction.Global;
using Silky.SkyApm.Diagnostics.Transaction.Participant;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace Silky.SkyApm.Diagnostics.Transaction
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddSkyApmSilkyTransaction(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services
                .AddSingleton<ITracingDiagnosticProcessor, ParticipantTransactionTracingDiagnosticProcessor>();
            extensions.Services
                .AddSingleton<ITracingDiagnosticProcessor, GlobalTransactionTracingDiagnosticProcessor>();
            extensions.Services.AddSingleton<ISilkySegmentContextFactory, SilkySegmentContextFactory>();
            return extensions;
        }
    }
}