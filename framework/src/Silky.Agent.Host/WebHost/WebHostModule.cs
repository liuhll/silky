﻿using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DistributedLock.Redis;
using Silky.DotNetty;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.Http.Core;
using Silky.Http.CorsAccessor;
using Silky.Http.MiniProfiler;
using Silky.Http.RateLimit;
using Silky.Http.Swagger;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Monitor;
using Silky.Rpc.Proxy;
using Silky.Swagger.Gen;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(RpcMonitorModule),
        typeof(RpcCachingInterceptorModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(RedisTransactionRepositoryModule),
        typeof(RedisDistributedLockModule),
        typeof(SilkyHttpCoreModule),
        typeof(MiniProfilerModule),
        typeof(RateLimitModule),
        typeof(CorsModule),
        typeof(SwaggerModule),
        typeof(SwaggerGenModule)
    )]
    public abstract class WebHostModule : HostAgentModule
    {
        
    }
}