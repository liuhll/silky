using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Silky.Core.DbContext;
using Silky.EntityFrameworkCore.Extensions.DatabaseProvider;

namespace Silky.EntityFrameworkCore.ContextPool
{
    public class EfCoreDbContextPool : ISilkyDbContextPool, IDisposable
    {
        private readonly ILogger<EfCoreDbContextPool> _logger;

        /// <summary>
        /// 线程安全的数据库上下文集合
        /// </summary>
        private readonly ConcurrentDictionary<Guid, DbContext> _dbContexts;

        /// <summary>
        /// 登记错误的数据库上下文
        /// </summary>
        private readonly ConcurrentDictionary<Guid, DbContext> _failedDbContexts;

        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public EfCoreDbContextPool(ILogger<EfCoreDbContextPool> logger)
        {
            _logger = logger;
            _dbContexts = new ConcurrentDictionary<Guid, DbContext>();
            _failedDbContexts = new ConcurrentDictionary<Guid, DbContext>();
        }

        /// <summary>
        /// 数据库上下文事务
        /// </summary>
        public IDbContextTransaction DbContextTransaction { get; private set; }

        /// <summary>
        /// 获取所有数据库上下文
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<Guid, DbContext> GetDbContexts()
        {
            return _dbContexts;
        }

        /// <summary>
        /// 保存数据库上下文
        /// </summary>
        /// <param name="dbContext"></param>
        public void AddToPool(DbContext dbContext)
        {
            // 跳过非关系型数据库
            if (!dbContext.Database.IsRelational()) return;

            var instanceId = dbContext.ContextId.InstanceId;
            if (!_dbContexts.TryAdd(instanceId, dbContext)) return;

            // 订阅数据库上下文操作失败事件
            dbContext.SaveChangesFailed += (s, e) =>
            {
                // 排除已经存在的数据库上下文
                if (!_failedDbContexts.TryAdd(instanceId, dbContext)) return;

                // 当前事务
                dynamic context = s as DbContext;
                var database = context.Database as DatabaseFacade;
                var currentTransaction = database?.CurrentTransaction;

                // 只有事务不等于空且支持自动回滚
                if (!(currentTransaction != null && context.FailedAutoRollback == true)) return;

                // 获取数据库连接信息
                var connection = database.GetDbConnection();

                // 回滚事务
                currentTransaction.Rollback();

                // 打印事务回滚消息
                _logger.LogInformation($"[Connection Id: {context.ContextId}] / [Database: {connection.Database}]");
            };
        }

        /// <summary>
        /// 保存数据库上下文（异步）
        /// </summary>
        /// <param name="dbContext"></param>
        public Task AddToPoolAsync(DbContext dbContext)
        {
            AddToPool(dbContext);
            return Task.CompletedTask;
        }

        private void EnsureDbContextAddToPool()
        {
            if (_dbContexts.Any()) return;
            var defaultDbContextLocator = Penetrates.DbContextDescriptors.LastOrDefault();
            if (defaultDbContextLocator.Key == null) return;
            var dbContext = Db.GetDbContext(defaultDbContextLocator.Key);
            if (!_dbContexts.Values.Contains(dbContext))
            {
                AddToPool(dbContext);
            }
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <returns></returns>
        public int SavePoolNow()
        {
            // 查找所有已改变的数据库上下文并保存更改
            return _dbContexts
                .Where(u => u.Value != null && !CheckDbContextDispose(u.Value) && u.Value.ChangeTracker.HasChanges() &&
                            !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChanges()).Count();
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <returns></returns>
        public int SavePoolNow(bool acceptAllChangesOnSuccess)
        {
            // 查找所有已改变的数据库上下文并保存更改
            return _dbContexts
                .Where(u => u.Value != null && !CheckDbContextDispose(u.Value) && u.Value.ChangeTracker.HasChanges() &&
                            !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChanges(acceptAllChangesOnSuccess)).Count();
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default)
        {
            // 查找所有已改变的数据库上下文并保存更改
            var tasks = _dbContexts
                .Where(u => u.Value != null && !CheckDbContextDispose(u.Value) && u.Value.ChangeTracker.HasChanges() &&
                            !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChangesAsync(cancellationToken));

            // 等待所有异步完成
            var results = await Task.WhenAll(tasks);
            return results.Length;
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文（异步）
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            // 查找所有已改变的数据库上下文并保存更改
            var tasks = _dbContexts
                .Where(u => u.Value != null && !CheckDbContextDispose(u.Value) && u.Value.ChangeTracker.HasChanges() &&
                            !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

            // 等待所有异步完成
            var results = await Task.WhenAll(tasks);
            return results.Length;
        }

        /// <summary>
        /// 打开事务
        /// </summary>
        /// <param name="ensureTransaction"></param>
        /// <returns></returns>
        public void BeginTransaction(bool ensureTransaction = false)
        {
            // 判断是否启用了分布式环境事务，如果是，则跳过
            if (Transaction.Current != null) return;

            // 判断 dbContextPool 中是否包含DbContext，如果是，则使用第一个数据库上下文开启事务，并应用于其他数据库上下文
            EnsureTransaction: if (_dbContexts.Any())
            {
                // 如果共享事务不为空，则直接共享
                if (DbContextTransaction != null) goto ShareTransaction;

                // 先判断是否已经有上下文开启了事务
                var transactionDbContext = _dbContexts.FirstOrDefault(u => u.Value.Database.CurrentTransaction != null);

                DbContextTransaction = transactionDbContext.Value != null
                    ? transactionDbContext.Value.Database.CurrentTransaction
                    // 如果没有任何上下文有事务，则将第一个开启事务
                    : _dbContexts.First().Value.Database.BeginTransaction();

                // 共享事务
            ShareTransaction: ShareTransaction(DbContextTransaction.GetDbTransaction());

                // 打印事务实际开启信息
                _logger.LogInformation( "Began Transaction");
            }
            else
            {
                // 判断是否确保事务强制可用（此处是无奈之举）
                if (!ensureTransaction) return;

                var defaultDbContextLocator = Penetrates.DbContextDescriptors.LastOrDefault();
                if (defaultDbContextLocator.Key == null) return;

                // 创建一个新的上下文
                var newDbContext = Db.GetDbContext(defaultDbContextLocator.Key);
                DbContextTransaction = newDbContext.Database.BeginTransaction();
                goto EnsureTransaction;
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="withCloseAll">是否自动关闭所有连接</param>
        public void CommitTransaction(bool isManualSaveChanges = true, Exception? exception = default,
            bool withCloseAll = false)
        {
            // 判断是否启用了分布式环境事务，如果是，则跳过
            if (Transaction.Current != null) return;
            try
            {
                if (exception == null)
                {
                    // 将所有数据库上下文修改 SaveChanges();，这里另外判断是否需要手动提交
                    var hasChangesCount = !isManualSaveChanges ? SavePoolNow() : 0;

                    // 如果事务为空，则执行完毕后关闭连接
                    if (DbContextTransaction == null)
                    {
                        if (withCloseAll) CloseAll();
                        return;
                    }

                    // 提交共享事务
                    DbContextTransaction?.Commit();

                    // 打印事务提交消息
                    _logger.LogInformation($"Transaction Completed! Has {hasChangesCount} DbContext Changes.");
                }
                else
                {
                    throw exception;
                }
            }
            catch
            {
                // 回滚事务
                if (DbContextTransaction?.GetDbTransaction()?.Connection != null) DbContextTransaction?.Rollback();

                // 打印事务回滚消息
                _logger.LogError("Transaction Rollback");

                throw;
            }
            finally
            {
                if (DbContextTransaction?.GetDbTransaction()?.Connection != null)
                {
                    DbContextTransaction = null;
                    DbContextTransaction?.Dispose();
                }
            }


            // 关闭所有连接
            if (withCloseAll) CloseAll();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="withCloseAll">是否自动关闭所有连接</param>
        public void RollbackTransaction(bool withCloseAll = false)
        {
            // 判断是否启用了分布式环境事务，如果是，则跳过
            if (Transaction.Current != null) return;

            // 回滚事务
            if (DbContextTransaction?.GetDbTransaction()?.Connection != null) DbContextTransaction?.Rollback();
            DbContextTransaction?.Dispose();
            DbContextTransaction = null;

            // 打印事务回滚消息
            _logger.LogError("Transaction Rollback");

            // 关闭所有连接
            if (withCloseAll) CloseAll();
        }

        /// <summary>
        /// 释放所有数据库上下文
        /// </summary>
        public void CloseAll()
        {
            if (!_dbContexts.Any()) return;

            foreach (var item in _dbContexts)
            {
                if (CheckDbContextDispose(item.Value)) continue;

                var conn = item.Value.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) continue;

                conn.Close();
                // 打印数据库关闭信息
                _logger.LogInformation($"Connection Close()");
            }
        }

        /// <summary>
        /// 设置数据库上下文共享事务
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private void ShareTransaction(DbTransaction transaction)
        {
            // 跳过第一个数据库上下文并设置共享事务
            _ = _dbContexts
                .Where(u => u.Value != null && !CheckDbContextDispose(u.Value) &&
                            ((dynamic)u.Value).UseUnitOfWork == true && u.Value.Database.CurrentTransaction == null)
                .Select(u => u.Value.Database.UseTransaction(transaction))
                .Count();
        }

        /// <summary>
        /// 释放所有上下文
        /// </summary>
        public void Dispose()
        {
            _dbContexts.Clear();
        }

        /// <summary>
        /// 判断数据库上下文是否释放
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static bool CheckDbContextDispose(DbContext dbContext)
        {
            // 反射获取 _disposed 字段，判断数据库上下文是否已释放
            var _disposedField =
                typeof(DbContext).GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_disposedField == null) return false;

            var _disposed = Convert.ToBoolean(_disposedField.GetValue(dbContext));
            return _disposed;
        }
    }
}