using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Silky.HealthChecks.Rpc;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SilkyRpcHealthCheckBuilderExtensions
    {
        internal const string SILKYRPC_NAME = "SilkyRpc";

        public static IHealthChecksBuilder AddSilkyRpc(
            this IHealthChecksBuilder builder,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                SILKYRPC_NAME,
                sp => new SilkyRpcHealthCheck(
                    sp.GetRequiredService<IServerManager>(),
                    sp.GetRequiredService<IServerHealthCheck>(),
                    sp.GetRequiredService<ICurrentRpcToken>()),
                failureStatus,
                tags,
                timeout));
        }
    }
}