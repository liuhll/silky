using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public static class ServiceEntryExtensions
    {
        public static bool TryGetMethodInfo(this ServiceEntry serviceEntry, out MethodInfo methodInfo)
        {
            methodInfo = serviceEntry.MethodInfo;
            return true;
        }

        public static IEnumerable<object> CustomAttributes(this ServiceEntry serviceEntry)
        {
            if (serviceEntry.TryGetMethodInfo(out MethodInfo methodInfo))
            {
                return methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetCustomAttributes(true));
            }

            return Enumerable.Empty<object>();
        }

        [Obsolete("Use TryGetMethodInfo() and CustomAttributes() instead")]
        public static void GetAdditionalMetadata(this ServiceEntry serviceEntry,
            out MethodInfo methodInfo,
            out IEnumerable<object> customAttributes)
        {
            if (serviceEntry.TryGetMethodInfo(out methodInfo))
            {
                customAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetCustomAttributes(true));

                return;
            }

            customAttributes = Enumerable.Empty<object>();
        }

        internal static string RelativePathSansQueryString(this ServiceEntry serviceEntry)
        {
            return serviceEntry.Router.RoutePath?.Split('?').First();
        }
    }
}