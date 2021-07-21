using System.Collections.Generic;
using System.Data;

namespace Silky.EntityFrameworkCore.Values
{
    /// <summary>
    /// 存储过程输出返回值
    /// </summary>
    public sealed class ProcedureOutputResult : ProcedureOutputResult<DataSet>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcedureOutputResult() : base()
        {
        }
    }

    /// <summary>
    /// 存储过程输出返回值
    /// </summary>
    /// <typeparam name="TResult">泛型版本</typeparam>
    public class ProcedureOutputResult<TResult>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcedureOutputResult()
        {
            OutputValues = new List<ProcedureOutputValue>();
        }

        /// <summary>
        /// 输出值
        /// </summary>
        public IEnumerable<ProcedureOutputValue> OutputValues { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        public TResult Result { get; set; }
    }
}