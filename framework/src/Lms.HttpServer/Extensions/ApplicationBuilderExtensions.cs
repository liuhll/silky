using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.HttpServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseLmsExceptionHandler(this IApplicationBuilder application)
        {
            var webHostEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();
            var gatewayOptions = EngineContext.Current.Resolve<IOptions<GatewayOptions>>().Value;
            var useDetailedExceptionPage = gatewayOptions.DisplayFullErrorStack || webHostEnvironment.IsDevelopment();
            if (useDetailedExceptionPage)
            {
                //get detailed exceptions for developing and testing purposes
                application.UseDeveloperExceptionPage();
            }
            else
            {
                application.UseExceptionHandler(handler =>
                {
                    handler.Run(context =>
                    {
                        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                        if (exception == null)
                            return Task.CompletedTask;

                        context.Response.ContentType = "application/text;charset=utf-8";
                        context.Response.StatusCode = 500;
                        return context.Response.WriteAsync(exception.Message);
                    });
                });
            }
        }
    }
}