using Silky.Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Rpc.Configuration;

namespace Silky.Lms.Rpc
{
    public class RpcConfigureServices : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RpcOptions>()
                .Bind(configuration.GetSection(RpcOptions.Rpc));
            services.AddOptions<RegistryCenterOptions>()
                .Bind(configuration.GetSection(RegistryCenterOptions.RegistryCenter));
            services.AddOptions<GovernanceOptions>()
                .Bind(configuration.GetSection(GovernanceOptions.Governance));
            services.AddOptions<WebSocketOptions>()
                .Bind(configuration.GetSection(WebSocketOptions.WebSocket));
        }

        public int Order { get; } = 1;
    }
}