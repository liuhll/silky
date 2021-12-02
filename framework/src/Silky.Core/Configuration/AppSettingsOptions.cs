namespace Silky.Core.Configuration
{
    public class AppSettingsOptions
    {
        public static string AppSettings = "AppSettings";

        public AppSettingsOptions()
        {
            DisplayFullErrorStack = false;
            AutoValidationParameters = true;
            AppServiceInterfacePattern = "Application.Contracts";
            AppServicePattern = "Application";
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

        /// <summary>
        /// 应用服务接口扫描的目录
        /// </summary>
        public string AppServiceInterfaceDirectory { get; set; }

        /// <summary>
        /// 应用服务接口正则匹配模式
        /// </summary>
        public string AppServiceInterfacePattern { get; set; }

        /// <summary>
        /// 应用服务接口扫描的目录
        /// </summary>
        public string AppServiceDirectory { get; set; }

        /// <summary>
        /// 应用服务正则匹配模式
        /// </summary>
        public string AppServicePattern { get; set; }
    }
}