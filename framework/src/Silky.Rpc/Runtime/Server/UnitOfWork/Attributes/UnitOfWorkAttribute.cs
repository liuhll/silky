

using System;

namespace Silky.Rpc.Runtime.Server.UnitOfWork
{
    /// <summary>
    /// 工作单元配置特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnitOfWorkAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UnitOfWorkAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ensureTransaction"></param>
        public UnitOfWorkAttribute(bool ensureTransaction)
        {
            EnsureTransaction = ensureTransaction;
        }

        /// <summary>
        /// 确保事务可用
        /// <para>此方法为了解决静态类方式操作数据库的问题</para>
        /// </summary>
        public bool EnsureTransaction { get; set; } = true;
    }
}