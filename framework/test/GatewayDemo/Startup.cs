using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Lms.Http.SkyApm.Diagnostics;
using Silky.Lms.Rpc.SkyApm.Diagnostics;
using Silky.Lms.Swagger.SwaggerUI;

namespace GatewayDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSkyAPM(extensions =>
            {
                extensions
                    .AddSilkyHttpServer()
                    .AddSilkyRpc();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureLmsRequestPipeline();
            // app.UseEndpoints(endpoints=> endpoints.MapControllers());
        }

        private static void InjectMiniProfilerPlugin(SwaggerUIOptions swaggerUIOptions)
        {
            // 启用 MiniProfiler 组件
            var thisType = typeof(SwaggerUIOptions);
            var thisAssembly = thisType.Assembly;

            // 自定义 Swagger 首页
            var customIndex = $"Silky.Lms.Swagger.SwaggerUI.index-mini-profiler.html";
            swaggerUIOptions.IndexStream = () => thisAssembly.GetManifestResourceStream(customIndex);
        }
    }
}