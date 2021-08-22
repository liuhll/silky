using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Http.Core.Configuration;

namespace Microsoft.AspNetCore.Builder
{
    public static class CorsAccessorApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCorsAccessor(this IApplicationBuilder app)
        {
            var corsAccessorSettings = app.ApplicationServices.GetService<IOptions<GatewayOptions>>().Value;

            if (corsAccessorSettings.EnableCors)
            {
                app.UseCors(corsAccessorSettings.CorsAccessorSettings.PolicyName);
               
            }
            return app;
        }
    }
}