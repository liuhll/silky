using Silky.HttpServer.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class SilkyMiddlewareExtensions
    {
        public static IApplicationBuilder UseSilky(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SilkyMiddleware>();
        }
    }
}