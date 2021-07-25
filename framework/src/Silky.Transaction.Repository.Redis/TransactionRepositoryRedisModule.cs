using Autofac;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Repository.Redis
{
    [DependsOn(typeof(RedisCachingModule))]
    public class TransactionRepositoryRedisModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<RedisTransRepository>()
                .InstancePerDependency()
                .Named<ITransRepository>(TransRepositorySupport.Redis.ToString());
        }
    }
}