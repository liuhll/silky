using System;
using System.Collections.Generic;
using Silky.Core.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Rpc.Routing.Template
{
    internal static class TemplateHelper
    {
        private static IDictionary<HttpMethod, string> constraintDefualtMethods = new Dictionary<HttpMethod, string>()
        {
            { HttpMethod.Get, "Get" },
            { HttpMethod.Post, "Create" },
            { HttpMethod.Put, "Update" },
            { HttpMethod.Patch, "Update" },
            { HttpMethod.Delete, "Delete" },
        };


        public static string GenerateServerEntryTemplate(string routeTemplate,
            string methodEntryTemplate,
            HttpMethod httpMethod,
            bool isSpecify,
            string methodName)
        {
            var serverEntryTemplate = routeTemplate;
            if (isSpecify)
            {
                if (methodEntryTemplate.IsNullOrEmpty() && constraintDefualtMethods.ContainsKey(httpMethod))
                {
                    var constraintDefaultMethod = constraintDefualtMethods[httpMethod];
                    if (!constraintDefaultMethod.IsNullOrEmpty() &&
                        !methodName.StartsWith(constraintDefaultMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        serverEntryTemplate = $"{routeTemplate}/{methodName}";
                    }
                    else
                    {
                        methodEntryTemplate =
                            methodName.RemovePreFix(StringComparison.OrdinalIgnoreCase, constraintDefaultMethod);
                        serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                    }
                }
                else
                {
                    serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                }
            }
            else
            {
                var constraintDefaultMethod = constraintDefualtMethods[httpMethod];
                if (!constraintDefaultMethod.IsNullOrEmpty() &&
                    !methodName.StartsWith(constraintDefaultMethod, StringComparison.OrdinalIgnoreCase))
                {
                    serverEntryTemplate = $"{routeTemplate}/{methodName}";
                }

                if (!constraintDefaultMethod.IsNullOrEmpty() &&
                    methodName.StartsWith(constraintDefaultMethod, StringComparison.OrdinalIgnoreCase))
                {
                    methodEntryTemplate =
                        methodName.RemovePreFix(StringComparison.OrdinalIgnoreCase, constraintDefaultMethod);
                    serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                }
            }

            return serverEntryTemplate;
        }
    }
}