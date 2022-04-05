using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Http.Core.Executor;
using Silky.Rpc;
using Silky.Transaction.Interceptor;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyHttpCoreModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkyHttpCore();
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseSilkyWrapperResponse();
            application.UseSilkyWebSocketsProxy();
            application.UseSilkyWebServer();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultHttpExecutor>()
                .As<IHttpExecutor>()
                .InstancePerDependency()
                .AddInterceptors(
                    typeof(TransactionInterceptor)
                )
                ;
        }
    }
}