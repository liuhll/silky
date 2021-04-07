using System;
using System.Collections.Generic;
using Silky.Lms.Core.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Lms.Rpc.Routing.Template
{
    internal static class TemplateHelper
    {
        private static IDictionary<HttpMethod, string> constraintDefualtMethods = new Dictionary<HttpMethod, string>()
        {
            {HttpMethod.Get, "Get"},
            {HttpMethod.Post, "Create"},
            {HttpMethod.Put, "Update"},
            {HttpMethod.Patch, "Update"},
            {HttpMethod.Delete, "Delete"},
        };


        public static string GenerateServerEntryTemplate(string routeTemplate, string methodEntryTemplate,
            HttpMethod httpMethod, bool isSpecify, string methodName)
        {
            var serverEntryTemplate = routeTemplate;
            if (isSpecify)
            {
                if (methodEntryTemplate.IsNullOrEmpty() && constraintDefualtMethods.ContainsKey(httpMethod))
                {
                    var constraintDefualtMethod = constraintDefualtMethods[httpMethod];
                    if (!constraintDefualtMethod.IsNullOrEmpty() &&
                        !methodName.StartsWith(constraintDefualtMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        serverEntryTemplate = $"{routeTemplate}/{methodName}";
                    }
                    
                }
                else
                {
                    serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                }
            }
            else
            {
                var constraintDefualtMethod = constraintDefualtMethods[httpMethod];
                if (!constraintDefualtMethod.IsNullOrEmpty() &&
                    !methodName.StartsWith(constraintDefualtMethod, StringComparison.OrdinalIgnoreCase))
                {
                    serverEntryTemplate = $"{routeTemplate}/{methodName}";
                }
            }

            return serverEntryTemplate;
        }
    }
}