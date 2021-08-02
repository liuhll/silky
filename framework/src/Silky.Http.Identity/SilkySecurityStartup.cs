using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Identity.Authentication;
using Silky.Http.Identity.Authentication.Middlewares;

namespace Silky.Http.Identity
{
    public class SilkySecurityStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, SilkyAuthenticationHandler>(
                    JwtBearerDefaults.AuthenticationScheme,
                    o => { });
            services.AddAuthorization();
            
        }

        public int Order { get; } = -2;

        public void Configure(IApplicationBuilder application)
        {
            
            application.UseAuthentication();
            application.UseSilkyAuthentication();
            application.UseAuthorization();
           
        }
    }
}