using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Security.Authentication;

namespace Silky.Http.Security
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
            services.Configure<MvcOptions>(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        public int Order { get; } = -2;

        public void Configure(IApplicationBuilder application)
        {
            
            application.UseAuthentication();
            application.UseAuthorization();
           
        }
    }
}