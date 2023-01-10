using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public static class ParameterExtensions
    {
        public static ParameterInfo ParameterInfo(this RpcParameter rpcParameter)
        {
            return rpcParameter?.ParameterInfo;
        }


        public static IEnumerable<object> CustomAttributes(this RpcParameter rpcParameter)
        {
            var parameterInfo = rpcParameter.ParameterInfo();
            if (parameterInfo != null) return parameterInfo.GetCustomAttributes(true);

            return Enumerable.Empty<object>();
        }


        internal static bool IsFromPath(this RpcParameter rpcParameter)
        {
            return rpcParameter.From == ParameterFrom.Path;
        }

        internal static bool IsFromBody(this RpcParameter rpcParameter)
        {
            return rpcParameter.From == ParameterFrom.Body;
        }

        internal static bool IsFromForm(this RpcParameter rpcParameter)
        {
            return rpcParameter.From == ParameterFrom.Form || rpcParameter.From == ParameterFrom.File;
        }
    }
}