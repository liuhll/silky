using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Silky.HealthChecks.Rpc
{
    public class SilkyRpcHealthCheckResult
    {
        public HealthStatus Status { get; set; }

        public IReadOnlyCollection<ServerHealthData> HealthData { get; set; }
        public TimeSpan TotalDuration { get; set; }
    }
}