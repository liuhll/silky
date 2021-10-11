using System;

namespace Silky.Http.CorsAccessor.Configuration
{
    public class CorsAccessorOptions
    {
        internal const string CorsAccessor = "CorsAccessor";

        /// <summary>
        /// 策略名称
        /// </summary>
        public string PolicyName { get; set; }

        /// <summary>
        /// 允许来源域名，没有配置则允许所有来源
        /// </summary>
        public string[] WithOrigins { get; set; }

        /// <summary>
        /// 请求表头，没有配置则允许所有表头
        /// </summary>
        public string[] WithHeaders { get; set; }

        /// <summary>
        /// 响应标头
        /// </summary>
        public string[] WithExposedHeaders { get; set; }

        /// <summary>
        /// 设置跨域允许请求谓词，没有配置则允许所有
        /// </summary>
        public string[] WithMethods { get; set; }

        /// <summary>
        /// 跨域请求中的凭据
        /// </summary>
        public bool? AllowCredentials { get; set; }

        /// <summary>
        /// 设置预检过期时间
        /// </summary>
        public int? SetPreflightMaxAge { get; set; }

        public CorsAccessorOptions()
        {
            PolicyName ??= "App.CorsAccessor.Policy";
            WithOrigins ??= Array.Empty<string>();
            AllowCredentials ??= true;
        }
    }
}