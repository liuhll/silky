using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lms.Core.Exceptions
{
    public enum StatusCode
    {
        [Description("成功")]
        [IsResponseStatus]
        Success = 200,
        
        [Description("架构异常")]
        [IsResponseStatus]
        PlatformError = 500,
        
        [Description("路由解析异常")]
        RouteParseError = 502,
        
        // RouteMatchError = 503,
        //
        // DataAccessError = 504,
    }
}