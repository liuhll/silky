using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Parameter;

namespace Silky.Swagger.SwaggerGen.SwaggerGenerator
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