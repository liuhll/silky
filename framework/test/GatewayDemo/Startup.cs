using GatewayDemo.Authorization;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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
            services.AddTransient<IAuthorizationHandler, TestAuthorizationHandlerBase>();
            services.AddSilkyHttpServices();
            services.AddMessagePackCodec();
            services.AddHealthChecks()
                .AddSilkyRpc();
            services
                .AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("silkyrpc", "http://127.0.0.1:5002/healthz");
                    setup.SetEvaluationTimeInSeconds(60);
                })
                .AddInMemoryStorage();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
            // app.UseClientRateLimiting();
            // app.UseIpRateLimiting();
            app.UseSilkyWrapperResponse();
            app.UseResponseCaching();
            app.UseHttpsRedirection();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyIdentity();
            app.UseSilkyHttpServer();

            //   app.UseSilkyExceptionHandler();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI();
                // endpoints.MapSilkyRpcHealthChecks();

                endpoints.MapSilkyRpcServices();
            });
        }
    }
}