using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Silky.Core;
using Silky.Rpc.Runtime.Server;

namespace Silky.EntityFrameworkCore.MultiTenants.Extensions
{
    /// <summary>
    /// 多租户数据库上下文拓展
    /// </summary>
    public static class MultiTenantDbContextExtensions
    {
        /// <summary>
        /// 刷新多租户缓存
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RefreshTenantCache(this DbContext dbContext)
        {
            var silkySession = NullSession.Instance;

            // 缓存的 Key
            var tenantCachedKey = $"MULTI_TENANT:{silkySession.TenantId}";

            // 从内存缓存中移除多租户信息
            var distributedCache = EngineContext.Current.Resolve<IDistributedCache>();
            distributedCache.Remove(tenantCachedKey);
        }
    }
}