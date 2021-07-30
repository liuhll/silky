using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;

namespace Silky.GatewayHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule))]
    public class GatewayHostModule : WebHostModule
    {
        
    }
}