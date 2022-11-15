using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.Contexts.Builders;
using Silky.EntityFrameworkCore.Entities.Attributes;
using Silky.EntityFrameworkCore.Entities.Configures;
using Silky.EntityFrameworkCore.Locators;

namespace Silky.EntityFrameworkCore.Contexts
{
    /// <summary>
    /// 默认应用数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    public abstract class AbstractDbContext<TDbContext> : AbstractDbContext<TDbContext, MasterDbContextLocator>
        where TDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        protected AbstractDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
        }
    }

    public abstract class AbstractDbContext<TDbContext, TDbContextLocator> : DbContext
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        protected AbstractDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {
            ChangeTrackerEntities ??= new Dictionary<EntityEntry, PropertyValues>();
        }

        /// <summary>
        /// 数据库上下文提交更改之前执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        protected virtual void SavingChangesEvent(DbContextEventData eventData, InterceptionResult<int> result)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改成功执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        protected virtual void SavedChangesEvent(SaveChangesCompletedEventData eventData, int result)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改失败执行事件
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void SaveChangesFailedEvent(DbContextErrorEventData eventData)
        {
        }

        /// <summary>
        /// 数据库上下文初始化调用方法
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// 数据库上下文配置模型调用方法
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置数据库上下文实体
            AppDbContextBuilder.ConfigureDbContextEntity(modelBuilder, this, typeof(TDbContextLocator));
        }

        /// <summary>
        /// 新增或更新忽略空值（默认值）
        /// </summary>
        public virtual bool InsertOrUpdateIgnoreNullValues { get; protected set; } = false;

        /// <summary>
        /// 启用实体跟踪（默认值）
        /// </summary>
        public virtual bool EnabledEntityStateTracked { get; protected set; } = true;

        /// <summary>
        /// 启用实体数据更改监听
        /// </summary>
        public virtual bool EnabledEntityChangedListener { get; protected set; } = false;

        /// <summary>
        /// 保存失败自动回滚
        /// </summary>
        public virtual bool FailedAutoRollback { get; protected set; } = true;
        
        /// <summary>
        /// 正在更改并跟踪的数据
        /// </summary>
        private Dictionary<EntityEntry, PropertyValues> ChangeTrackerEntities { get; set; }

        /// <summary>
        /// 内部数据库上下文提交更改之前执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        internal void SavingChangesEventInner(DbContextEventData eventData, InterceptionResult<int> result)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener)
            {
                var dbContext = eventData.Context;

                // 获取获取数据库操作上下文，跳过贴了 [NotChangedListener] 特性的实体
                ChangeTrackerEntities = (dbContext).ChangeTracker.Entries()
                    .Where(u => !u.Entity.GetType().IsDefined(typeof(SuppressChangedListenerAttribute), true) &&
                                (u.State == EntityState.Added || u.State == EntityState.Modified ||
                                 u.State == EntityState.Deleted)).ToDictionary(u => u, u => u.GetDatabaseValues());

                AttachEntityChangedListener(eventData.Context, "OnChanging", ChangeTrackerEntities);
            }

            SavingChangesEvent(eventData, result);
        }

        /// <summary>
        /// 内部数据库上下文提交更改成功执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        internal void SavedChangesEventInner(SaveChangesCompletedEventData eventData, int result)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener)
                AttachEntityChangedListener(eventData.Context, "OnChanged", ChangeTrackerEntities);

            SavedChangesEvent(eventData, result);
        }

        /// <summary>
        /// 内部数据库上下文提交更改失败执行事件
        /// </summary>
        /// <param name="eventData"></param>
        internal void SaveChangesFailedEventInner(DbContextErrorEventData eventData)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener)
                AttachEntityChangedListener(eventData.Context, "OnChangeFailed", ChangeTrackerEntities);

            SaveChangesFailedEvent(eventData);
        }

        /// <summary>
        /// 附加实体改变监听
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="triggerMethodName"></param>
        /// <param name="changeTrackerEntities"></param>
        private static void AttachEntityChangedListener(DbContext dbContext, string triggerMethodName,
            Dictionary<EntityEntry, PropertyValues> changeTrackerEntities = null)
        {
            // 获取所有改变的类型
            var entityChangedTypes = AppDbContextBuilder.DbContextLocatorCorrelationTypes[typeof(TDbContextLocator)]
                .EntityChangedTypes;
            if (!entityChangedTypes.Any()) return;

            // 遍历所有的改变的实体
            foreach (var trackerEntities in changeTrackerEntities)
            {
                var entryEntity = trackerEntities.Key;
                var entity = entryEntity.Entity;

                // 获取该实体类型的种子配置
                var entitiesTypeByChanged = entityChangedTypes
                    .Where(u => u.GetInterfaces()
                        .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntityChangedListener<>)) &&
                                  i.GenericTypeArguments.Contains(entity.GetType())));

                if (!entitiesTypeByChanged.Any()) continue;

                // 通知所有的监听类型
                foreach (var entityChangedType in entitiesTypeByChanged)
                {
                    var OnChangeMethod = entityChangedType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(u => u.Name == triggerMethodName
                                    && u.GetParameters().Length > 0
                                    && u.GetParameters().First().ParameterType == entity.GetType())
                        .FirstOrDefault();
                    if (OnChangeMethod == null) continue;

                    var instance = Activator.CreateInstance(entityChangedType);

                    // 对 OnChanged 进行特别处理
                    if (triggerMethodName.Equals("OnChanged"))
                    {
                        // 获取实体旧值
                        var oldEntity = trackerEntities.Value?.ToObject();

                        OnChangeMethod.Invoke(instance,
                            new object[]
                                { entity, oldEntity, dbContext, typeof(TDbContextLocator), entryEntity.State });
                    }
                    else
                    {
                        OnChangeMethod.Invoke(instance,
                            new object[] { entity, dbContext, typeof(TDbContextLocator), entryEntity.State });
                    }
                }
            }
        }
    }
}