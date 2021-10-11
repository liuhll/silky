using System.Collections.Generic;
using System.ComponentModel;
using Silky.Core.Extensions;

namespace Silky.Swagger.SwaggerGen.SwaggerGenerator
{
    public enum ResponsesCode
    {
        [Description("成功")] Success = 200,

        [Description("业务异常")] BusinessError = 500,

        [Description("鉴权失败")] UnAuthorized = 401,

        [Description("架构异常")] ArchitectureError = 501,
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