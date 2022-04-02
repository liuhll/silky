using System.IO.Compression;
using System.Linq;
using GatewayDemo.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core.Middlewares;


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
            // services.AddSwaggerDocuments();
            // services.AddSilkyMiniProfiler();
            // // services.AddDashboard();
            // services.AddSilkyIdentity();
            // services.AddSilkySkyApm();
            // services.AddRouting();
            // services.AddMessagePackCodec();
            // var redisOptions = Configuration.GetRateLimitRedisOptions();
            // services.AddClientRateLimit(redisOptions);
            // services.AddIpRateLimit(redisOptions);
            // services.AddResponseCaching();
            // services.AddMvc();
            // services.AddSilkyHttpCore();
            // services.AddTransient<IAuthorizationHandler, TestAuthorizationHandlerBase>();
            services.AddSilkySkyApm();
            services.AddSilkyHttpServices<TestAuthorizationHandler>();
            services.AddSilkyIdentity();
            services.AddMessagePackCodec();
            services.AddHealthChecks()
                .AddSilkyRpc()
                .AddSilkyGateway();

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
            services
                .AddHealthChecksUI()
                .AddInMemoryStorage();
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
            app.UseSilkyRpcHealthCheck()
                .UseSilkyGatewayHealthCheck()
                .UseHealthChecksPrometheusExporter("/metrics");

            app.UseRouting();
            // app.UseClientRateLimiting();
            // app.UseIpRateLimiting();
            app.UseResponseCaching();
            app.UseHttpsRedirection();
            app.UseSilkyIdentity();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyWebServer();
            // app.UseSilkyWrapperResponse();
            // app.UseAuditing();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI();
                endpoints.MapSilkyRpcServices();
            });
        }
    }
}