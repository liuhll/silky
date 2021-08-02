using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authentication.Middlewares;
using Silky.Http.Identity.Authorization;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Http.Identity.Authorization.Providers;
using Silky.Rpc.Security;

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
            services.AddAuthorization(options =>
            {
                var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                var defaultPolicy = defaultAuthBuilder.RequireAuthenticatedUser().Build();
                options.DefaultPolicy = defaultPolicy;
            });
            services.Configure<MvcOptions>(options => { options.Filters.Add(new AuthorizeFilter()); });
            services.AddTransient<IAuthorizationHandler, SilkyAuthorizationHandler>();
              services.AddTransient<IAuthorizationPolicyProvider, SilkyAuthorizationPolicyProvider>();
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