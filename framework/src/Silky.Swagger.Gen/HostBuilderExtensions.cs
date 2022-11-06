using Microsoft.Extensions.DependencyInjection;
using Silky.Swagger.Gen;

namespace Microsoft.Extensions.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder RegisterSwaggerDocInfo(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
           
            services.AddSwaggerRegisterInfoGen(context.Configuration);
            services.AddSwaggerInfoService(context.Configuration);

            services.AddHostedService<InitRegisterSwaggerHostedService>();
        });
        return hostBuilder;
    }
}