using Microsoft.Extensions.Hosting;

namespace Lms.Rpc
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseRpcServer(this IHostBuilder builder)
        {
            return builder.ConfigureServices(config =>
            {
                // config.AddHostedService<RpcServerHostedService>();
            });
        }
    }
}