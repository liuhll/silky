using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core;
using Silky.Http.MiniProfiler;
using Demo.EntityFrameworkCore.DbContexts;

namespace SilkyAppHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) 
        {
            services
                .AddSilkyHttpCore()
#if (hosttype=="gateway")        
                .AddDashboard()    
#endif           
                .AddResponseCaching()
                .AddHttpContextAccessor()
                .AddRouting()
                .AddSilkyIdentity()
                .AddSilkyMiniProfiler()
                .AddSilkyCaching()
                .AddSilkySkyApm()
                .AddSwaggerDocuments()
                .AddMessagePackCodec();

#if (hosttype=="gateway")                  
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<DefaultContext>(); },
                "SilkyApp.Database.Migrations");
#endif
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("ContainerDev"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
                app.UseMiniProfiler();
            }
#if (hosttype=="gateway")   
           app.UseDashboard();
#endif           
            app.UseRouting();
            app.UseSilkyWrapperResponse();
            app.UseResponseCaching();
            app.UseHttpsRedirection();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyIdentity();
#if (hosttype=="gateway")   
           app.UseSilkyHttpServer();
#endif              
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSilkyRpcServices();
            });
        }
    }
}
