
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core.DbContext;
using Silky.Core.DependencyInjection;
using Silky.EntityFrameworkCore;
using Silky.EntityFrameworkCore.ContextPool;
using Silky.EntityFrameworkCore.Contexts.Dynamic;
using Silky.EntityFrameworkCore.Contexts.Enums;
using Silky.EntityFrameworkCore.Extensions.DatabaseProvider;
using Silky.EntityFrameworkCore.Locators;
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
    /// 注册默认数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <param name="services">服务提供器</param>
    public static IServiceCollection RegisterDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        return services.RegisterDbContext<TDbContext, MasterDbContextLocator>();
    }

    /// <summary>
    /// 注册数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    /// <param name="services">服务提供器</param>
    public static IServiceCollection RegisterDbContext<TDbContext, TDbContextLocator>(this IServiceCollection services)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        // 存储数据库上下文和定位器关系
        Penetrates.DbContextDescriptors.AddOrUpdate(typeof(TDbContextLocator), typeof(TDbContext), (key, value) => typeof(TDbContext));

        // 注册数据库上下文
        services.TryAddScoped<TDbContext>();

        return services;
    }

    /// <summary>
    /// 通过定位器解析上下文
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dbContextLocator"></param>
    /// <returns></returns>
    private static DbContext ResolveDbContext(IServiceProvider provider, Type dbContextLocator)
    {
        // 判断数据库上下文定位器是否绑定
        Penetrates.CheckDbContextLocator(dbContextLocator, out var dbContextType);

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
        dbContextPool!.AddToPool(dbContext);

        return dbContext;
    }
    }
}