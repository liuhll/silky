using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Template
{
    internal static class TemplateHelper
    {
        private static IDictionary<HttpMethod, ICollection<string>> constraintDefualtMethods =
            new Dictionary<HttpMethod, ICollection<string>>()
            {
                { HttpMethod.Get, new List<string>() { "GetBy", "GetFor", "GetFrom", "Get" } },
                { HttpMethod.Post, new List<string>() { "CreateOrUpdate", "CreateOrModify", "Create", "Add" } },
                {
                    HttpMethod.Put, new List<string>() { "CreateOrUpdate", "CreateOrModify", "Update", "Put", "Modify" }
                },
                {
                    HttpMethod.Patch,
                    new List<string>() { "CreateOrUpdate", "CreateOrModify", "Update", "Put", "Modify" }
                },
                { HttpMethod.Delete, new List<string>() { "Delete" } },
            };

        private const string constraintRemovePostFix = "Async";

        public static string GenerateServerEntryTemplate(string routeTemplate,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            HttpMethodInfo httpMethodInfo,
            bool isRestful,
            string methodName)
        {
            var serverEntryTemplate = routeTemplate;
            var methodEntryTemplate = httpMethodInfo.Template;
            if (httpMethodInfo.IsSpecify)
            {
                if (methodEntryTemplate == null)
                {
                    methodEntryTemplate = isRestful
                        ? methodName.RemovePostFix(StringComparison.OrdinalIgnoreCase, constraintRemovePostFix)
                        : methodName;

                    if (constraintDefualtMethods.TryGetValue(httpMethodInfo.HttpMethod, out var constraintMethods) &&
                        constraintMethods.Any(cm => methodName.StartsWith(cm, StringComparison.OrdinalIgnoreCase)) &&
                        isRestful)
                    {
                        var conditionConstraintMethods = constraintMethods.Where(cm =>
                            methodName.StartsWith(cm, StringComparison.OrdinalIgnoreCase));
                        foreach (var conditionConstraintMethod in conditionConstraintMethods)
                        {
                            methodEntryTemplate =
                                methodEntryTemplate.RemovePreFix(StringComparison.OrdinalIgnoreCase,
                                    conditionConstraintMethod);
                        }
                    }

                    serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                    var parameterPathTemplate = GetParametersTemplate(parameterDescriptors);
                    if (!parameterPathTemplate.IsNullOrEmpty())
                    {
                        serverEntryTemplate += "/" + parameterPathTemplate;
                    }
                }
                else
                {
                    serverEntryTemplate = !methodEntryTemplate.IsNullOrWhiteSpace()
                        ? $"{routeTemplate}/{methodEntryTemplate}"
                        : $"{routeTemplate}";
                }
            }
            else
            {
                var constraintMethods = constraintDefualtMethods[httpMethodInfo.HttpMethod];
                methodEntryTemplate = isRestful
                    ? methodName.RemovePostFix(StringComparison.OrdinalIgnoreCase, constraintRemovePostFix)
                    : methodName;
                if (constraintMethods != null && isRestful)
                {
                    var conditionConstraintMethods = constraintMethods.Where(cm =>
                        methodName.StartsWith(cm, StringComparison.OrdinalIgnoreCase));
                    foreach (var conditionConstraintMethod in conditionConstraintMethods)
                    {
                        methodEntryTemplate =
                            methodEntryTemplate.RemovePreFix(StringComparison.OrdinalIgnoreCase,
                                conditionConstraintMethod);
                    }
                }

                serverEntryTemplate = $"{routeTemplate}/{methodEntryTemplate}";
                var parameterPathTemplate = GetParametersTemplate(parameterDescriptors);
                if (!parameterPathTemplate.IsNullOrEmpty())
                {
                    serverEntryTemplate += "/" + parameterPathTemplate;
                }
            }

            return serverEntryTemplate;
        }

        private static string GetParametersTemplate(IReadOnlyList<ParameterDescriptor> parameterDescriptors)
        {
            var pathParameters = parameterDescriptors.Where(p => p.From == ParameterFrom.Path);
            if (pathParameters.Any())
            {
                var parameterPathTemplate =
                    string.Join("/", pathParameters.Select(p => p.PathTemplate)).TrimEnd('/');
                return parameterPathTemplate;
            }
            return null;
        }
    }
}