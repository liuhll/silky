using Lms.Rpc.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lms.Rpc
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseRpcServer(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hosting,services) =>
            {
                // config.AddHostedService<RpcServerHostedService>();
                services.AddOptions<RpcOptions>()
                    .Bind(hosting.Configuration.GetSection(RpcOptions.Rpc));
                services.AddOptions<RegistryCenterOptions>()
                    .Bind(hosting.Configuration.GetSection(RegistryCenterOptions.RegistryCenter));

            });
        }
    }
}