using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core;

namespace GatewayDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSilkyHttpCore()
                .AddSwaggerDocuments()
                .AddRouting()
                .AddDashboard();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
            }

            app.UseDashboard();
            app.UseRouting();
            app.UseWebSockets();
            app.UseSilkyWebSocketsProxy();
           // app.UseSilkyWrapperResponse();
            app.UseSilkyHttpServer();
            app.UseEndpoints(endpoints => { endpoints.MapSilkyRpcServices(); });
        }
    }
}