using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Core;
using Silky.Jwt;

namespace Silky.Http.Identity
{
    [DependsOn(typeof(JwtModule), typeof(SilkyHttpCoreModule))]
    public class IdentityModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkyIdentity();
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseSilkyIdentity();
        }
    }
}