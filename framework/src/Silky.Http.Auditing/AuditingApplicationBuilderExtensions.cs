using Silky.Http.Auditing.Middlewares;

namespace Microsoft.AspNetCore.Builder;

public static class AuditingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAuditing(this IApplicationBuilder application)
    {
        application.UseMiddleware<AuditingMiddleware>();
        return application;
    }
}