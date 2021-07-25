using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;
using Silky.Transaction.Repository.Redis;

namespace GatewayDemo
{
    [DependsOn(typeof(MessagePackModule),
        typeof(TransactionRepositoryRedisModule),
        typeof(SilkySkyApmAgentModule))]
    public class GatewayHostModule : WebHostModule
    {
    }
}