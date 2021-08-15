using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.Http.Identity;
using Silky.SkyApm.Agent;

namespace SilkyApp.GatewayHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule),typeof(IdentityModule),typeof(MessagePackModule))]
    public class GatewayHostModule : WebHostModule
    {
        
    }
}