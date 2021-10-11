using System;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.EntityFrameworkCore.Entities;
using Silky.EntityFrameworkCore.Locators;

namespace Silky.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// 多数据库仓储
    /// </summary>
    /// <typeparam name="TDbContextLocator"></typeparam>
    public partial class DbRepository<TDbContextLocator> : IDbRepository<TDbContextLocator>
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DbRepository()
        {
        }

        /// <summary>
        /// 切换实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual IRepository<TEntity, TDbContextLocator> Change<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return EngineContext.Current.Resolve<IRepository<TEntity, TDbContextLocator>>();
        }

        /// <summary>
        /// 获取 Sql 操作仓储
        /// </summary>
        /// <returns></returns>
        public virtual ISqlRepository<TDbContextLocator> Sql()
        {
            return EngineContext.Current.Resolve<ISqlRepository<TDbContextLocator>>();
        }
    }
}