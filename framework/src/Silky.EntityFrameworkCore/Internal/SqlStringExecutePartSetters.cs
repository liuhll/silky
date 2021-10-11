using System;
using Silky.EntityFrameworkCore.Locators;

namespace Silky.EntityFrameworkCore
{
    /// <summary>
    /// 构建 Sql 字符串执行部件
    /// </summary>
    public sealed partial class SqlStringExecutePart
    {
        /// <summary>
        /// 设置 Sql 字符串
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlStringExecutePart SetSqlString(string sql)
        {
            SqlString = sql;
            return this;
        }

        /// <summary>
        /// 设置 ADO.NET 超时时间
        /// </summary>
        /// <param name="timeout">单位秒</param>
        /// <returns></returns>
        public SqlStringExecutePart SetCommandTimeout(int timeout)
        {
            Timeout = timeout;
            return this;
        }


        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <returns></returns>
        public SqlStringExecutePart Change<TDbContextLocator>()
            where TDbContextLocator : class, IDbContextLocator
        {
            DbContextLocator = typeof(TDbContextLocator) ?? typeof(MasterDbContextLocator);
            return this;
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <returns></returns>
        public SqlStringExecutePart Change(Type dbContextLocator)
        {
            DbContextLocator = dbContextLocator ?? typeof(MasterDbContextLocator);
            return this;
        }
    }
}