using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Silky.GatewayHost
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
            services.AddSilkyHttpServices();
            services.AddSilkySkyApm();
            services.AddMessagePackCodec();
            services.AddHealthChecks()
                .AddSilkyRpc();
            services
                .AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("silkyrpc", "http://127.0.0.1:5000/healthz");
                    setup.SetEvaluationTimeInSeconds(60);
                })
                .AddInMemoryStorage();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.EnvironmentName == "ContainerDev")
            {
               
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
                app.UseMiniProfiler();
            }
            app.UseSerilogRequestLogging();
            app.UseDashboard();
            app.UseHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                })
                .UseHealthChecksPrometheusExporter("/metrics");
            app.UseRouting();
            app.UseSilkyWrapperResponse();
            // app.UseClientRateLimiting();
            // app.UseIpRateLimiting();
            app.UseResponseCaching();
            app.UseHttpsRedirection();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyExceptionHandler();
            app.UseSilkyIdentity();
            app.UseSilkyHttpServer();
            // app.ConfigureSilkyRequestPipeline();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI();
                endpoints.MapSilkyRpcServices();
            });
        }
    }
}