using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.Transaction.Repository.Redis;

namespace GatewayDemo
{
    [DependsOn(typeof(MessagePackModule),
        typeof(TransactionRepositoryRedisModule))]
    public class GatewayHostModule : WebHostModule
    {
    }
}