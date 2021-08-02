using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using Silky.Http.Identity;
using Silky.SkyApm.Agent;

namespace Silky.GatewayHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule),typeof(IdentityModule))]
    public class GatewayHostModule : WebHostModule
    {
        
    }
}