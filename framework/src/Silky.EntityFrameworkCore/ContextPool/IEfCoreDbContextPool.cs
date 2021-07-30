using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Silky.Rpc.Runtime.Server.ContextPool;

namespace Silky.EntityFrameworkCore.ContextPool
{
    /// <summary>
    /// 数据库上下文池
    /// </summary>
    public interface IEfCoreDbContextPool : IDbContextPool
    {
        /// <summary>
        /// 数据库上下文事务
        /// </summary>
        IDbContextTransaction DbContextTransaction { get; }

        /// <summary>
        /// 获取所有数据库上下文
        /// </summary>
        /// <returns></returns>
        ConcurrentDictionary<Guid, DbContext> GetDbContexts();

        /// <summary>
        /// 保存数据库上下文
        /// </summary>
        /// <param name="dbContext"></param>
        void AddToPool(DbContext dbContext);

        /// <summary>
        /// 保存数据库上下文（异步）
        /// </summary>
        /// <param name="dbContext"></param>
        Task AddToPoolAsync(DbContext dbContext);
    }
}