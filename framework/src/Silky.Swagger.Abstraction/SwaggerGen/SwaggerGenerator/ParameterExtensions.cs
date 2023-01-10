using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public static class ParameterExtensions
    {
        public static ParameterInfo ParameterInfo(this RpcParameter apiRpcParameter)
        {
            return apiRpcParameter?.ParameterInfo;
        }


        public static IEnumerable<object> CustomAttributes(this RpcParameter apiRpcParameter)
        {
            var parameterInfo = apiRpcParameter.ParameterInfo();
            if (parameterInfo != null) return parameterInfo.GetCustomAttributes(true);

            return Enumerable.Empty<object>();
        }


        internal static bool IsFromPath(this RpcParameter apiRpcParameter)
        {
            return apiRpcParameter.From == ParameterFrom.Path;
        }

        internal static bool IsFromBody(this RpcParameter apiRpcParameter)
        {
            return apiRpcParameter.From == ParameterFrom.Body;
        }

        internal static bool IsFromForm(this RpcParameter apiRpcParameter)
        {
            return apiRpcParameter.From == ParameterFrom.Form || apiRpcParameter.From == ParameterFrom.File;
        }
    }
}