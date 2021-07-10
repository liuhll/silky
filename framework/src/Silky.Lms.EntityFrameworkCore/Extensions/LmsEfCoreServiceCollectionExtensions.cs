using System;
using Microsoft.EntityFrameworkCore;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.EntityFrameworkCore.ContextPools;
using Silky.Lms.EntityFrameworkCore.Contexts;
using Silky.Lms.EntityFrameworkCore.Contexts.Dynamic;
using Silky.Lms.EntityFrameworkCore.Contexts.Enums;
using Silky.Lms.EntityFrameworkCore.Extensions.DatabaseProvider;
using Silky.Lms.EntityFrameworkCore.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LmsEfCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddLmsDbContext<TDbContext>(
            this IServiceCollection services,
            Action<IServiceCollection> configure = null)
            where TDbContext : LmsDbContext<TDbContext>
        {
            configure?.Invoke(services);
            // 解析数据库上下文
            services.AddTransient(provider =>
            {
                DbContext dbContextResolve(Type locator, ITransientDependency transient)
                {
                    return ResolveDbContext(provider, locator);
                }
                return (Func<Type, ITransientDependency, DbContext>)dbContextResolve;
            });

            services.AddScoped(provider =>
            {
                DbContext dbContextResolve(Type locator, IScopedDependency transient)
                {
                    return ResolveDbContext(provider, locator);
                }
                return (Func<Type, IScopedDependency, DbContext>)dbContextResolve;
            });
            return services;
        }
        
        /// <summary>
        /// 通过定位器解析上下文
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="locator"></param>
        /// <returns></returns>
        private static DbContext ResolveDbContext(IServiceProvider provider, Type locator)
        {
            // 判断定位器是否绑定了数据库上下文
            var isRegistered = Penetrates.DbContextWithLocatorCached.TryGetValue(locator, out var dbContextType);
            if (!isRegistered) throw new InvalidOperationException($"The DbContext for locator `{locator.FullName}` binding was not found.");

            // 动态解析数据库上下文
            var dbContext = provider.GetService(dbContextType) as DbContext;

            // 实现动态数据库上下文功能，刷新 OnModelCreating
            var dbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);
            if (dbContextAttribute?.Mode == DbContextMode.Dynamic)
            {
                DynamicModelCacheKeyFactory.RebuildModels();
            }

            // 添加数据库上下文到池中
            var dbContextPool = provider.GetService<IDbContextPool>();
            dbContextPool?.AddToPool(dbContext);

            return dbContext;
        }
    }
}