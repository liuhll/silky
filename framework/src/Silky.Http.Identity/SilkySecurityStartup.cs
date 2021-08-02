using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Identity.Authentication.Handlers;
using Silky.Http.Identity.Authentication.Middlewares;

namespace Silky.Http.Identity
{
    public class SilkySecurityStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public int Order { get; } = -2;

        public void Configure(IApplicationBuilder application)
        {
            if (application.ApplicationServices.GetService<SilkyAuthenticationHandler>() != null &&
                application.ApplicationServices.GetService<IAuthorizationHandler>() != null)
            {
                application.UseRouting();
                application.UseAuthentication();
                application.UseSilkyAuthentication();
                application.UseAuthorization();
            }
        }
    }
}