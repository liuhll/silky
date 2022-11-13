using System.IO.Compression;
using System.Linq;
using GatewayDemo.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core;
using Silky.Http.MiniProfiler;


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
            services.AddSilkySkyApm();
            
            // services
            //     .AddSilkyHttpCore()
            //     .AddDashboard()
            //     .AddResponseCaching()
            //     .AddHttpContextAccessor()
            //     .AddRouting()
            //     .AddSilkyIdentity<TestAuthorizationHandler>()
            //     .AddSilkyMiniProfiler()
            //     .AddSwaggerDocuments();

            services.AddSilkyHttpServices<TestAuthorizationHandler>();
            
            services.AddCorsAccessor();

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                // options.Providers.Add<CustomCompressionProvider>();
                // .Append(TItem) is only available on Core.
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });

                ////Example of using excluded and wildcard MIME types:
                ////Compress all MIME types except various media types, but do compress SVG images.
                //options.MimeTypes = new[] { "*/*", "image/svg+xml" };
                //options.ExcludedMimeTypes = new[] { "image/*", "audio/*", "video/*" };
            });
            // services
            //     .AddHealthChecksUI()
            //     .AddInMemoryStorage();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("ContainerDev"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
                app.UseMiniProfiler();
            }

            //  app.UseSerilogRequestLogging();
            app.UseDashboard();
            // app.UseSilkyRpcHealthCheck()
            //     .UseSilkyGatewayHealthCheck()
            //     .UseHealthChecksPrometheusExporter("/metrics");

            app.UseRouting();
            // app.UseClientRateLimiting();
            // app.UseIpRateLimiting();
            app.UseResponseCaching();
            // app.UseHttpsRedirection();
            app.UseSilkyWebSocketsProxy();
            // app.UseSilkyWebServer();
            app.UseSilkyWrapperResponse();
            app.UseSilkyIdentity();
            app.UseAuditing();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapHealthChecksUI();
                endpoints.MapSilkyRpcServices();
                // endpoints.MapSilkyServiceEntries();
                // endpoints.MapSilkyTemplateServices();
                // endpoints.MapSilkyDashboardServices();
            });
        }
    }
}