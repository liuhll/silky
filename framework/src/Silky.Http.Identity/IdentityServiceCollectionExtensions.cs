using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Http.Identity;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authorization.Providers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddSilkyIdentity(this IServiceCollection services,
            Action<AuthorizationOptions> configure = null)
        {
            return services
                .AddSilkyAuthentication()
                .AddSilkyAuthorization(configure);
        }

        public static IServiceCollection AddSilkyIdentity<TAuthorizationHandler>(this IServiceCollection services,
            Action<AuthorizationOptions> configure = null)
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            return services
                .AddSilkyAuthentication()
                .AddSilkyAuthorization<TAuthorizationHandler>(configure);
        }

        public static IServiceCollection AddSilkyAuthentication(this IServiceCollection services)
        {
            if (!services.IsAddedImplementationType(typeof(SilkyAuthenticationHandler)))
            {
                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, SilkyAuthenticationHandler>(
                        JwtBearerDefaults.AuthenticationScheme, null);
                services.AddJwt();
            }

            return services;
        }

        public static IServiceCollection AddSilkyAuthorization<TAuthorizationHandler>(this IServiceCollection services,
            Action<AuthorizationOptions> configure = null)
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.TryAddSingleton<IAuthorizationHandler, TAuthorizationHandler>();
            services.AddSilkyAuthorization(configure);
            return services;
        }

        public static IServiceCollection AddSilkyAuthorization(this IServiceCollection services,
            Action<AuthorizationOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.TryAddSingleton<IAuthorizationPolicyProvider, SilkyAuthorizationPolicyProvider>();
            if (configure == null)
            {
                services.AddAuthorization();
            }
            else
            {
                services.AddAuthorization(configure);
            }

            return services;
        }
    }
}