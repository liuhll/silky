using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core;
using Silky.Http.MiniProfiler;

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
                .AddSwaggerDocuments();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
                app.UseMiniProfiler();
            }
#if (hosttype=="gateway")   
            app.UseDashboard();
#endif           
            app.UseRouting();
#if (hosttype=="gateway")             
            app.UseSilkyWrapperResponse();
#endif            
            app.UseResponseCaching();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyIdentity();
            app.UseSilkyWebServer();           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSilkyRpcServices();
            });
        }
    }
}
