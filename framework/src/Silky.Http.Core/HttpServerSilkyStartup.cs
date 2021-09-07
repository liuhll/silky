using System;
using Silky.Core;
using Silky.Http.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Http.Core
{
    public class HttpServerSilkyStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
          
        }

        public async void Configure(IApplicationBuilder application)
        {
              application.UseSilky();
              application.RegisterGateway();
        }

        public int Order { get; } = Int32.MaxValue;
    }
}