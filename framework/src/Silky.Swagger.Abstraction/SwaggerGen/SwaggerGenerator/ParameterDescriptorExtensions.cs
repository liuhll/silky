using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public static class ParameterDescriptorExtensions
    {
        public static ParameterInfo ParameterInfo(this ParameterDescriptor apiParameter)
        {
            return apiParameter?.ParameterInfo;
        }


        public static IEnumerable<object> CustomAttributes(this ParameterDescriptor apiParameter)
        {
            var parameterInfo = apiParameter.ParameterInfo();
            if (parameterInfo != null) return parameterInfo.GetCustomAttributes(true);

            return Enumerable.Empty<object>();
        }


        internal static bool IsFromPath(this ParameterDescriptor apiParameter)
        {
            return apiParameter.From == ParameterFrom.Path;
        }

        internal static bool IsFromBody(this ParameterDescriptor apiParameter)
        {
            return apiParameter.From == ParameterFrom.Body;
        }

        internal static bool IsFromForm(this ParameterDescriptor apiParameter)
        {
            return apiParameter.From == ParameterFrom.Form;
        }
    }
}