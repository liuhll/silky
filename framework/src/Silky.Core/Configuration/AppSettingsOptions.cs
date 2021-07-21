namespace Silky.Core.Configuration
{
    public class AppSettingsOptions
    {
        
        public static string AppSettings = "AppSettings";
        
        /// <summary>
        /// 是否记录 EFCore Sql 执行命令日志
        /// </summary>
        public bool? LogEntityFrameworkCoreSqlExecuteCommand { get; set; }
    }
}