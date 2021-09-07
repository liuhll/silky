using GatewayDemo.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using Silky.Http.CorsAccessor;
using Silky.SkyApm.Agent;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;

namespace GatewayDemo
{
    [DependsOn( /*typeof(MessagePackModule),*/
        typeof(TransactionRepositoryRedisModule),
        typeof(SkyApmAgentModule),
        typeof(TransactionTccModule),
//        typeof(IdentityModule),
//        typeof(DashboardModule),
// #if DEBUG
//         typeof(SwaggerModule),
//         typeof(MiniProfilerModule),
// #endif
    
        typeof(CorsModule)
    )]
    public class GatewayHostModule : WebHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IAuthorizationHandler, TestAuthorizationHandler>();
        }
    }
}