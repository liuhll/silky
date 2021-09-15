using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;

namespace Silky.Codec
{
    [DependsOn(typeof(RpcModule))]
    public class MessagePackModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessagePackCodec();
        }
    }
}