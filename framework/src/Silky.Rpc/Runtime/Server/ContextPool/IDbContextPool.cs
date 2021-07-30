using System;
using System.Threading;
using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server.ContextPool
{
    public interface IDbContextPool
    {
        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <returns></returns>
        int SavePoolNow();

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <returns></returns>
        int SavePoolNow(bool acceptAllChangesOnSuccess);

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文（异步）
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

        /// <summary>
        /// 打开事务
        /// </summary>
        /// <param name="ensureTransaction"></param>
        /// <returns></returns>
        void BeginTransaction(bool ensureTransaction = false);

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="isManualSaveChanges"></param>
        /// <param name="exception"></param>
        /// <param name="withCloseAll">是否自动关闭所有连接</param>
        void CommitTransaction(bool isManualSaveChanges = true, Exception exception = default, bool withCloseAll = false);

        /// <summary>
        /// 关闭所有数据库链接
        /// </summary>
        void CloseAll();
    }
}