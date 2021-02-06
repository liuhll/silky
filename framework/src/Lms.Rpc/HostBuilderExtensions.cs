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
                services.AddHostedService<RpcServerHostedService>();

            });
        }
    }
}