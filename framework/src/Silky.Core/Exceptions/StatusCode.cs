using System.ComponentModel;

namespace Silky.Core.Exceptions
{
    public enum StatusCode
    {
        [Description("成功")] Success = 200,

        [Description("架构异常")] PlatformError = 500,

        [Description("路由解析异常")] RouteParseError = 502,

        [Description("未找到服务路由")] NotFindServiceRoute = 503,

        [Description("未找到可用的服务地址")] NotFindServiceRouteAddress = 504,

        [Description("非平台异常")] UnPlatformError = 505,

        [Description("通信异常")] CommunicatonError = 506,
        
        [Description("未找到本地服务条目")] NotFindLocalServiceEntry = 507,

        [Description("业务异常")] [IsBusinessException]
        BusinessError = 1000,

        [Description("验证异常")] [IsBusinessException]
        ValidateError = 1001,
        
        [Description("Tcc分布式事务异常")] [IsBusinessException]
        TransactionError = 511,

        [Description("超过最大并发量")] OverflowMaxRequest = 507,

        [Description("UnServiceKeyImplementation")]
        UnServiceKeyImplementation = 508,

        [Description("rpc通信认证失败")] RpcUnAuthentication = 509,

        [Description("缓存拦截存在异常")] CachingInterceptError = 510,

        [Description("未认证")] [IsUnAuthorizedException]
        UnAuthentication = 401,

        [Description("未授权")] [IsUnAuthorizedException]
        UnAuthorization = 402,
        [Description("禁止外网访问")] FuseProtection = 406,
    }
}