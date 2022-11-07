using System.Collections.Generic;
using System.ComponentModel;
using Silky.Core.Extensions;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public enum ResponsesCode
    {
        [Description("成功")] Success = 200,

        [Description("框架异常")] FrameworkException = 500,
        
        [Description("业务异常")] BusinessError = 1000,

        [Description("验证异常")] ValidateError = 1001,

        [Description("用户友好类异常")] UserFriendly = 1002,

        [Description("未登陆系统")] UnAuthentication = 401,

        [Description("未授权访问")] UnAuthorization = 403,

        // [Description("未找到路由信息")] NotFound = 404,
        //
        // [Description("校验token失败")] IssueTokenError = 4011,
        //
        // [Description("禁止外网访问")] FuseProtection = 406,

        [Description("请求异常")] BadRequest = 400,
    }

    public static class ResponsesCodeHelper
    {
        public static IDictionary<ResponsesCode, string> GetAllCodes()
        {
            var codes = typeof(ResponsesCode).GetEnumSources<ResponsesCode>();
            return codes;
        }
    }
}