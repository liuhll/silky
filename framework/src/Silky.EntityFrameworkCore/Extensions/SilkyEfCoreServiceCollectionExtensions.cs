using System;
using Microsoft.EntityFrameworkCore;
using Silky.Core.DbContext;
using Silky.Core.DependencyInjection;
using Silky.EntityFrameworkCore;
using Silky.EntityFrameworkCore.ContextPool;
using Silky.EntityFrameworkCore.Contexts.Dynamic;
using Silky.EntityFrameworkCore.Contexts.Enums;
using Silky.EntityFrameworkCore.Extensions.DatabaseProvider;
using Silky.EntityFrameworkCore.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SilkyEfCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseAccessor(
            this IServiceCollection services,
            Action<IServiceCollection> configure = null, string migrationAssemblyName = default)
        {
            // 设置迁移类库名称
            if (!string.IsNullOrWhiteSpace(migrationAssemblyName)) Db.MigrationAssemblyName = migrationAssemblyName;

            configure?.Invoke(services);

            // 注册数据库上下文池
            services.AddScoped<ISilkyDbContextPool, EfCoreDbContextPool>();

            // 注册 Sql 仓储
            services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

            // 注册 Sql 非泛型仓储
            services.AddScoped<ISqlRepository, SqlRepository>();

            // 注册多数据库上下文仓储
            services.AddScoped(typeof(IRepository<,>), typeof(EFCoreRepository<,>));

            // 注册泛型仓储
            services.AddScoped(typeof(IRepository<>), typeof(EFCoreRepository<>));

            // 注册主从库仓储
            services.AddScoped(typeof(IMSRepository), typeof(MSRepository));
            services.AddScoped(typeof(IMSRepository<>), typeof(MSRepository<>));
            services.AddScoped(typeof(IMSRepository<,>), typeof(MSRepository<,>));
            services.AddScoped(typeof(IMSRepository<,,>), typeof(MSRepository<,,>));
            services.AddScoped(typeof(IMSRepository<,,,>), typeof(MSRepository<,,,>));
            services.AddScoped(typeof(IMSRepository<,,,,>), typeof(MSRepository<,,,,>));
            services.AddScoped(typeof(IMSRepository<,,,,,>), typeof(MSRepository<,,,,,>));
            services.AddScoped(typeof(IMSRepository<,,,,,,>), typeof(MSRepository<,,,,,,>));
            services.AddScoped(typeof(IMSRepository<,,,,,,,>), typeof(MSRepository<,,,,,,,>));

            // 注册非泛型仓储
            services.AddScoped<IRepository, EFCoreRepository>();

            // 注册多数据库仓储
            services.AddScoped(typeof(IDbRepository<>), typeof(DbRepository<>));

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
            if (!isRegistered)
                throw new InvalidOperationException(
                    $"The DbContext for locator `{locator.FullName}` binding was not found.");

            // 动态解析数据库上下文
            var dbContext = provider.GetService(dbContextType) as DbContext;

            // 实现动态数据库上下文功能，刷新 OnModelCreating
            var dbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);
            if (dbContextAttribute?.Mode == DbContextMode.Dynamic)
            {
                DynamicModelCacheKeyFactory.RebuildModels();
            }

            // 添加数据库上下文到池中
            var dbContextPool = provider.GetService<ISilkyDbContextPool>() as EfCoreDbContextPool;
            dbContextPool?.AddToPool(dbContext);

            return dbContext;
        }

        /// <summary>
        /// 启动自定义租户类型
        /// </summary>
        /// <param name="services"></param>
        /// <param name="onTableTenantId">基于表的多租户Id名称</param>
        /// <returns></returns>
        public static IServiceCollection CustomizeMultiTenants(this IServiceCollection services,
            string onTableTenantId = default)
        {
            Db.CustomizeMultiTenants = true;
            if (!string.IsNullOrWhiteSpace(onTableTenantId)) Db.OnTableTenantId = onTableTenantId;

            return services;
        }
    }
}