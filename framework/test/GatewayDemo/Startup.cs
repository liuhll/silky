using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Silky.Lms.Core;
using Silky.Lms.Rpc.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Silky.Lms.HttpServer.Configuration;
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lms Gateway Demo", Version = "v1" });
                c.MultipleServiceKey();
                var applicationAssemblies = EngineContext.Current.TypeFinder.GetAssemblies()
                    .Where(p => p.FullName.Contains("Application"));
                foreach (var applicationAssembly in applicationAssemblies)
                {
                    var xmlFile = $"{applicationAssembly.GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                }

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lms Gateway Demo v1");
                    InjectMiniProfilerPlugin(c);
                });
            }
            app.ConfigureLmsRequestPipeline();
           // app.UseEndpoints(endpoints=> endpoints.MapControllers());
        }
        
        private static void InjectMiniProfilerPlugin(SwaggerUIOptions swaggerUIOptions)
        {
            // 启用 MiniProfiler 组件
            var thisType =typeof(SwaggerUIOptions);
            var thisAssembly = thisType.Assembly;

            // 自定义 Swagger 首页
            var customIndex = $"Silky.Lms.Swagger.SwaggerUI.index-mini-profiler.html";
            swaggerUIOptions.IndexStream = () => thisAssembly.GetManifestResourceStream(customIndex);
        }
    }
}