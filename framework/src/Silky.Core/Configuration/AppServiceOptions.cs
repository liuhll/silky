namespace Silky.Core.Configuration;

public class AppServiceOptions
{
    public AppServiceOptions()
    {
        AppServiceInterfacePattern = "Application.Contracts";
        AppServicePattern = "Application";
    }

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