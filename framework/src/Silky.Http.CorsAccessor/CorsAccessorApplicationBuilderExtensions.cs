using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Http.CorsAccessor.Configuration;

namespace Microsoft.AspNetCore.Builder
{
    public static class CorsAccessorApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCorsAccessor(this IApplicationBuilder app,
            Action<CorsPolicyBuilder> corsPolicyBuilderHandler = default)
        {
            var corsAccessorSettings = app.ApplicationServices.GetService<IOptions<CorsAccessorOptions>>().Value;
            _ = corsPolicyBuilderHandler == null
                ? app.UseCors(corsAccessorSettings.PolicyName)
                : app.UseCors(corsPolicyBuilderHandler);
            return app;
        }
    }
}