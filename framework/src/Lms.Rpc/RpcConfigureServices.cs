using Lms.Core;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Rpc
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
        }

        public int Order { get; } = 1;
    }
}