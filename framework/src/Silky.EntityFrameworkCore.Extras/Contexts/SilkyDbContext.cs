using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silky.Core;
using Silky.Core.Runtime.Session;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Entities;
using Silky.EntityFrameworkCore.Entities.Configures;
using Silky.EntityFrameworkCore.Extras.Entities;
using Silky.EntityFrameworkCore.MultiTenants.Dependencies;
using Silky.Hero.Common.EntityFrameworkCore.Entities;

namespace Silky.EntityFrameworkCore.Extras.Contexts;

public abstract class SilkyDbContext<TDbContext> : AbstractDbContext<TDbContext>, IModelBuilderFilter, IMultiTenantOnTable
    where TDbContext : DbContext
{
    protected SilkyDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
        // 启用实体数据更改监听
        EnabledEntityChangedListener = true;

        // 忽略空值更新
        InsertOrUpdateIgnoreNullValues = true;
    }

    private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
        = typeof(SilkyDbContext<TDbContext>)
            .GetMethod(
                nameof(ConfigureBaseProperties),
                BindingFlags.Instance | BindingFlags.NonPublic
            );

    public object GetTenantId()
    {
        var currentTenantId = EngineContext.Current.Resolve<ICurrentTenantId>();
        return currentTenantId.TenantId;
    }

    public bool RealDeleteFlag { get; set; } = false;

    protected override void SavingChangesEvent(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // 获取当前事件对应上下文
        var dbContext = eventData.Context;
        // 获取所有更改，删除，新增的实体，但排除审计实体（避免死循环）
        var entities = dbContext.ChangeTracker.Entries()
            .Where(u => u.State == EntityState.Modified || u.State == EntityState.Deleted ||
                        u.State == EntityState.Added)
            .ToList();
        if (!entities.Any()) return;

        var session = NullSession.Instance;
        long? userId = session.UserId != null ? long.Parse(session.UserId.ToString()!) : null;
        long? tenantId = session.TenantId != null ? long.Parse(session.TenantId.ToString()!) : null;
        foreach (var entity in entities)
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    if (entity.Entity is ICreatedObject createdObject)
                    {
                        createdObject.CreatedTime = DateTimeOffset.Now;
                        createdObject.CreatedBy = userId;
                    }

                    if (entity.Entity is IHasTenantObject tenantObject)
                    {
                        tenantObject.TenantId ??= tenantId;
                    }

                    if (entity.Entity is ISoftDeletedObject deletedObject1)
                    {
                        deletedObject1.IsDeleted = false;
                    }

                    break;
                case EntityState.Deleted:

                    if (!RealDeleteFlag && entity.Entity is ISoftDeletedObject deletedObject2)
                    {
                        deletedObject2.IsDeleted = true;
                        deletedObject2.DeletedBy = userId;
                        deletedObject2.DeletedTime = DateTimeOffset.Now;
                        entity.State = EntityState.Modified;
                    }

                    break;
                case EntityState.Modified:
                    if (entity.Entity is ICreatedObject)
                    {
                        // 排除创建人
                        entity.Property(nameof(AuditedEntity.CreatedBy)).IsModified = false;
                        // 排除创建日期
                        entity.Property(nameof(AuditedEntity.CreatedTime)).IsModified = false;
                    }

                    if (entity.Entity is IUpdatedObject updatedObject)
                    {
                        updatedObject.UpdatedTime = DateTimeOffset.Now;
                        updatedObject.UpdatedBy = userId;
                    }

                    break;
            }
        }
    }

    public void OnCreating(ModelBuilder modelBuilder, EntityTypeBuilder entityBuilder, DbContext dbContext,
        Type dbContextLocator)
    {
        ConfigureBasePropertiesMethodInfo
            .MakeGenericMethod(entityBuilder.Metadata.ClrType)
            .Invoke(this, new object[] { modelBuilder, entityBuilder.Metadata });
    }

    protected virtual void ConfigureBaseProperties<TEntity>(ModelBuilder modelBuilder,
        IMutableEntityType mutableEntityType)
        where TEntity : class
    {
        if (mutableEntityType.IsOwned())
        {
            return;
        }

        if (!typeof(IEntity).IsAssignableFrom(typeof(TEntity)) && !typeof(IPrivateEntity).IsAssignableFrom(typeof(Entity)))
        {
            return;
        }

        ConfigureGlobalFilters<TEntity>(modelBuilder, mutableEntityType);
    }

    protected virtual void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder,
        IMutableEntityType mutableEntityType)
        where TEntity : class
    {
        if (mutableEntityType.BaseType == null && ShouldFilterEntity<TEntity>(mutableEntityType))
        {
            var filterExpression = CreateFilterExpression<TEntity>();
            if (filterExpression != null)
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
            }
        }
    }

    protected virtual bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
    {
        if (typeof(IHasTenantObject).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        if (typeof(ISoftDeletedObject).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return false;
    }

    protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
        where TEntity : class
    {
        Expression<Func<TEntity, bool>> expression = null;

        if (typeof(ISoftDeletedObject).IsAssignableFrom(typeof(TEntity)))
        {
            expression = e => !EF.Property<bool>(e, nameof(ISoftDeletedObject.IsDeleted));
        }

        if (typeof(IHasTenantObject).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>>
                multiTenantFilter = e => EF.Property<object>(e, nameof(IHasTenantObject.TenantId)) == GetTenantId();
            expression = expression == null ? multiTenantFilter : CombineExpressions(expression, multiTenantFilter);
        }

        return expression;
    }

    protected virtual Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expression1,
        Expression<Func<T, bool>> expression2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expression1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expression1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expression2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expression2.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
    }

    class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
            {
                return _newValue;
            }

            return base.Visit(node);
        }
    }
}