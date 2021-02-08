using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lms.Core.Exceptions
{
    public enum StatusCode
    {
        [Description("成功")] [IsResponseStatus] Success = 200,

        [Description("架构异常")] [IsResponseStatus]
        PlatformError = 500,

        [Description("路由解析异常")] RouteParseError = 502,

        [Description("未找到服务路由")] [IsResponseStatus]
        NotFindServiceRoute = 503,

        [Description("未找到可用的服务地址")] [IsResponseStatus]
        NotFindServiceRouteAddress = 504,

        [Description("非平台异常")] UnPlatformError = 505,

        [Description("业务异常")] BusinessError = 1000,

        [Description("严重异常")] ValidateError = 1001
    }
}