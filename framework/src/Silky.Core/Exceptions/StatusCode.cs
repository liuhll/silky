using System.ComponentModel;

namespace Silky.Core.Exceptions
{
    public enum StatusCode
    {
        [Description("成功")] Success = 200,

        [Description("框架异常")] FrameworkException = 500,

        [Description("路由解析异常")] RouteParseError = 503,

        [Description("未找到服务路由")] NotFindServiceRoute = 404,

        [Description("未找到可用的服务地址")] NotFindServiceRouteAddress = 504,

        [Description("非框架异常")] NonSilkyException = 505,

        [Description("通信异常")] CommunicatonError = 506,

        [Description("未找到本地服务条目")] NotFindLocalServiceEntry = 507,

        [Description("未找到失败回调的实现类")] NotFindFallbackInstance = 508,

        [Description("执行失败回调方法失败")] FallbackExecFail = 509,

        [Description("分布式事务异常")] TransactionError = 511,

        [Description("Tcc分布式事务异常")] TccTransactionError = 512,

        [Description("不存在服务条目")] NotFindServiceEntry = 513,

        [Description("超过最大并发量")] OverflowMaxRequest = 514,

        [Description("超过最大并发量")] OverflowMaxServerHandle = 515,

        [Description("UnServiceKeyImplementation")]
        UnServiceKeyImplementation = 516,

        [Description("rpc通信认证失败")] RpcUnAuthentication = 517,

        [Description("缓存拦截存在异常")] CachingInterceptError = 518,

        [Description("执行超时")] Timeout = 519,

        [Description("服务器异常")] ServerError = 520,

        [Description("没有内容")] NoContent = 521,

        [Description("业务异常")] [IsBusinessException]
        BusinessError = 1000,

        [Description("验证异常")] [IsUserFriendlyException]
        ValidateError = 1001,

        [Description("用户友好类异常")] [IsUserFriendlyException]
        UserFriendly = 1002,

        [Description("未认证")] [IsUnAuthorizedException]
        UnAuthentication = 401,

        [Description("未授权")] [IsUnAuthorizedException]
        UnAuthorization = 403,

        [Description("未找到路由信息")] NotFound = 404,

        [Description("校验token失败")] [IsUnAuthorizedException]
        IssueTokenError = 4011,

        [Description("禁止外网访问")] FuseProtection = 406,
        
        [Description("请求异常")] BadRequest = 400,
    }
}