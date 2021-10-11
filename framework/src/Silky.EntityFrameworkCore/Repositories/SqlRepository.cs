using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.DbContext;
using Silky.Core.DependencyInjection;
using Silky.EntityFrameworkCore.ContextPool;
using Silky.EntityFrameworkCore.Locators;

namespace Silky.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// Sql 操作仓储实现
    /// </summary>
    public partial class SqlRepository : SqlRepository<MasterDbContextLocator>, ISqlRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供器</param>
        public SqlRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    /// <summary>
    /// Sql 操作仓储实现
    /// </summary>
    public partial class SqlRepository<TDbContextLocator> : PrivateSqlRepository, ISqlRepository<TDbContextLocator>
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlRepository(IServiceProvider serviceProvider) : base(typeof(TDbContextLocator))
        {
        }
    }

    /// <summary>
    /// 私有 Sql 仓储
    /// </summary>
    public partial class PrivateSqlRepository : IPrivateSqlRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContextLocator"></param>
        public PrivateSqlRepository(Type dbContextLocator)
        {
            // 解析数据库上下文
            var dbContextResolve = EngineContext.Current.Resolve<Func<Type, IScopedDependency, DbContext>>();
            var dbContext = dbContextResolve(dbContextLocator, default);
            DynamicContext = Context = dbContext;

            // 初始化数据库相关数据
            Database = Context.Database;
        }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        public virtual dynamic DynamicContext { get; }

        /// <summary>
        /// 数据库操作对象
        /// </summary>
        public virtual DatabaseFacade Database { get; }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual ISqlRepository<TChangeDbContextLocator> Change<TChangeDbContextLocator>()
            where TChangeDbContextLocator : class, IDbContextLocator
        {
            return EngineContext.Current.Resolve<ISqlRepository<TChangeDbContextLocator>>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetService<TService>()
            where TService : class
        {
            return EngineContext.Current.Resolve<TService>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetRequiredService<TService>()
            where TService : class
        {
            return EngineContext.Current.Resolve<TService>();
        }

        /// <summary>
        /// 将仓储约束为特定仓储
        /// </summary>
        /// <typeparam name="TRestrainRepository">特定仓储</typeparam>
        /// <returns>TRestrainRepository</returns>
        public virtual TRestrainRepository Constraint<TRestrainRepository>()
            where TRestrainRepository : class, IPrivateRootRepository
        {
            var type = typeof(TRestrainRepository);
            if (!type.IsInterface || typeof(IPrivateRootRepository) == type || type.Name.Equals(nameof(IRepository)) ||
                (type.IsGenericType && type.GetGenericTypeDefinition().Name.Equals(nameof(IRepository))))
            {
                throw new InvalidCastException("Invalid type conversion.");
            }

            return this as TRestrainRepository;
        }

        /// <summary>
        /// 确保工作单元（事务）可用
        /// </summary>
        public virtual void EnsureTransaction()
        {
            // 获取数据库上下文
            var dbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>() as EfCoreDbContextPool;
            if (dbContextPool == null) return;

            // 追加上下文
            dbContextPool.AddToPool(Context);
            // 开启事务
            dbContextPool.BeginTransaction();
        }
    }
}