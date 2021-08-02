using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authorization;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Http.Identity.Authorization.Providers;
using Silky.Jwt;

namespace Silky.Http.Identity
{
    [DependsOn(typeof(JwtModule))]
    public class IdentityModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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
    }
}