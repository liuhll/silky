using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.DistributedLock.Redis;
using Silky.Rpc.Configuration;
using StackExchange.Redis;

namespace Silky.RegistryCenter.Consul.HealthCheck;

public class ConsulHealthCheckService : IHealthCheckService
{
    private ConcurrentDictionary<string, int> _serverInstanceUnHealthCache = new();

    private GovernanceOptions _governanceOptions;
    private readonly IRedisDistributedLockProvider _distributedLockProvider;

    public ConsulHealthCheckService(IRedisDistributedLockProvider distributedLockProvider)
    {
        _distributedLockProvider = distributedLockProvider;
        _governanceOptions = EngineContext.Current.GetOptionsMonitor<GovernanceOptions>(((options, s) =>
        {
            _governanceOptions = options;
        }));
    }


    public async Task<string[]> Check(IConsulClient consulClient, string service)
    {
        var unHealthServiceIds = new List<string>();

        var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
        using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
        var @lock = _distributedLockProvider.Create(connection.GetDatabase(), $"ConsulHealthCheck:{service}");
        await using var handle = await @lock.TryAcquireAsync();
        if (handle == null) return unHealthServiceIds.ToArray();
        var result = await consulClient.Health.Checks(service);
        foreach (var healthCheck in result.Response)
        {
            var unHealthCount = _serverInstanceUnHealthCache.GetValueOrDefault(healthCheck.ServiceID, 0);
            if (healthCheck.Status.Equals(HealthStatus.Passing))
            {
                unHealthCount = 0;
            }
            else if (healthCheck.Status.Equals(HealthStatus.Critical))
            {
                unHealthServiceIds.Add(healthCheck.ServiceID);
                unHealthCount += 1;
            }

            _serverInstanceUnHealthCache.AddOrUpdate(healthCheck.ServiceID, unHealthCount, (k, v) => unHealthCount);
            if (unHealthCount >= _governanceOptions.UnHealthAddressTimesAllowedBeforeRemoving)
            {
                await consulClient.Agent.ServiceDeregister(healthCheck.ServiceID);
                _serverInstanceUnHealthCache.TryRemove(healthCheck.ServiceID, out _);
            }
        }

        return unHealthServiceIds.ToArray();
    }
}