using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Silky.Core.Serialization;
using Silky.HealthChecks.Rpc;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SilkyRpcHealthCheckBuilderExtensions
    {
        internal const string SILKYRPC_NAME = "SilkyRpc";

        internal const string SILKYGATEWAT_NAME = "SilkyGateway";

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
                    sp.GetRequiredService<ICurrentRpcToken>(),
                    sp.GetRequiredService<ISerializer>(),
                    sp.GetRequiredService<IHttpHandleDiagnosticListener>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    sp.GetRequiredService<IServiceEntryLocator>(),
                    sp.GetRequiredService<IRpcEndpointMonitor>(),
                    sp.GetRequiredService<IOptions<GovernanceOptions>>()
                ),
                failureStatus,
                tags,
                timeout));
        }

        public static IHealthChecksBuilder AddSilkyGateway(
            this IHealthChecksBuilder builder,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                SILKYGATEWAT_NAME,
                sp => new SilkyGatewayHealthCheck(
                    sp.GetRequiredService<IServerManager>(),
                    sp.GetRequiredService<IServerHealthCheck>(),
                    sp.GetRequiredService<ICurrentRpcToken>(),
                    sp.GetRequiredService<ISerializer>(),
                    sp.GetRequiredService<IHttpHandleDiagnosticListener>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    sp.GetRequiredService<IServiceEntryLocator>(),
                    sp.GetRequiredService<IRpcEndpointMonitor>(),
                    sp.GetRequiredService<IOptions<GovernanceOptions>>()
                ),
                failureStatus,
                tags,
                timeout));
        }
    }
}