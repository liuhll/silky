using System;
using Lms.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.HttpServer
{
    public class HttpServerLmsStartup : ILmsStartup
    {

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
           
        }

        public void Configure(IApplicationBuilder application)
        {
            application.Run( async context =>
            {
                await EngineContext.Current.Resolve<HttpMessageReceivedHandler>().Handle(context);
            });
        }

        public int Order { get; } = Int32.MinValue;
    }
}