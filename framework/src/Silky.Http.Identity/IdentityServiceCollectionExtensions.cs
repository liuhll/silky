using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Silky.Http.Identity;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authorization;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Http.Identity.Authorization.Providers;

namespace  Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddSilkyIdentity(
            this IServiceCollection services)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, SilkyAuthenticationHandler>(
                    JwtBearerDefaults.AuthenticationScheme, null);
            services.AddAuthorization();
            services.AddTransient<IAuthorizationHandler, DefaultSilkyAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, SilkyAuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationMiddlewareResultHandler, SilkyAuthorizationMiddlewareResultHandler>();
            return services;
        }
    }
}