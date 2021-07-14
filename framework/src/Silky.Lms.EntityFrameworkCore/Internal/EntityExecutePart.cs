
using System;
using Silky.Lms.EntityFrameworkCore.Entities;
using Silky.Lms.EntityFrameworkCore.Locators;

namespace Silky.Lms.EntityFrameworkCore
{
    /// <summary>
    /// 实体执行部件
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed partial class EntityExecutePart<TEntity>
        where TEntity : class, IPrivateEntity, new()
    {
        /// <summary>
        /// 实体
        /// </summary>
        public TEntity Entity { get; private set; }

        /// <summary>
        /// 数据库上下文定位器
        /// </summary>
        public Type DbContextLocator { get; private set; } = typeof(MasterDbContextLocator);
        
    }
}