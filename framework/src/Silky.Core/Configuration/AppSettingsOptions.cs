namespace Silky.Core.Configuration
{
    public class AppSettingsOptions
    {
        public static string AppSettings = "AppSettings";

        public AppSettingsOptions()
        {
            DisplayFullErrorStack = false;
            AutoValidationParameters = true;
        }

        /// <summary>
        /// 是否显示堆栈信息
        /// </summary>
        public bool DisplayFullErrorStack { get; set; }

        /// <summary>
        /// 是否记录 EFCore Sql 执行命令日志
        /// </summary>
        public bool? LogEntityFrameworkCoreSqlExecuteCommand { get; set; }


        /// <summary>
        /// 是否自动校验输入参数
        /// </summary>
        public bool AutoValidationParameters { get; set; }
        
    }
}