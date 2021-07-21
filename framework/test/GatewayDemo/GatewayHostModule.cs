using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;

namespace GatewayDemo
{
    [DependsOn(typeof(MessagePackModule))]
    public class GatewayHostModule : WebHostModule
    {
        
    }
}