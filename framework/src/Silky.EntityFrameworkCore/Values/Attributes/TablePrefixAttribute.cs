namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// 配置表名称前缀
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TablePrefixAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prefix"></param>
        public TablePrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get; set; }
    }
}