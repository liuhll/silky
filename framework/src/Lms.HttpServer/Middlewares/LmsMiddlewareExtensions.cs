using Lms.HttpServer.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class LmsMiddlewareExtensions
    {
        public static IApplicationBuilder UseLms(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LmsMiddleware>();
        }
    }
}