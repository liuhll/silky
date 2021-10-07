using Silky.Core.Modularity;
using Silky.HealthChecks.Rpc;

namespace Silky.Http.Dashboard
{
    [DependsOn(typeof(SilkyRpcHealthCheck))]
    public class DashboardModule : HttpSilkyModule
    {
    }
}