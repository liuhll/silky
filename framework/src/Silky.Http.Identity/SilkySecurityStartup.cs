using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authentication.Middlewares;
using Silky.Http.Identity.Authorization;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Http.Identity.Authorization.Providers;

namespace Silky.Http.Identity
{
    public class SilkySecurityStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, SilkyAuthenticationHandler>(
                    JwtBearerDefaults.AuthenticationScheme, null);
            services.AddAuthorization();
            services.AddTransient<IAuthorizationHandler, DefaultSilkyAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, SilkyAuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationMiddlewareResultHandler, SilkyAuthorizationMiddlewareResultHandler>();
        }

        public int Order { get; } = -2;

        public void Configure(IApplicationBuilder application)
        {
            application.UseRouting();
            application.UseAuthentication();
            application.UseSilkyAuthentication();
            application.UseAuthorization();
        }
    }
}