using Silky.Http.Core.Middlewares;

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