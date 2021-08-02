using Microsoft.AspNetCore.Builder;

namespace Silky.Http.Identity.Authentication.Middlewares
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSilkyAuthentication(this IApplicationBuilder application)
        {
            application.UseMiddleware<SilkyAuthenticationMiddleware>();
            return application;
        }
    }
}