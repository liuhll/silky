using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;

namespace GatewayDemo
{
    [DependsOn(typeof(MessagePackModule),
        typeof(TransactionRepositoryRedisModule),
        typeof(SilkySkyApmAgentModule),
        typeof(TransactionTccModule)
    )]
    public class GatewayHostModule : WebHostModule
    {
    }
}