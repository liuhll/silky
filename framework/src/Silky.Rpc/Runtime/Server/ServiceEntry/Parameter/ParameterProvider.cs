using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Runtime.Server
{
    public class ParameterProvider : IParameterProvider
    {
        public IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethod httpMethod)
        {
            var parameterDescriptors = new List<ParameterDescriptor>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                var parameterDescriptor = CreateParameterDescriptor(methodInfo, parameter, httpMethod);
                if (parameterDescriptor.From == ParameterFrom.Body &&
                    parameterDescriptors.Any(p => p.From == ParameterFrom.Body))
                {
                    throw new SilkyException("Only one parameter of Request Body is allowed to be set");
                }

                parameterDescriptors.Add(parameterDescriptor);
            }

            return parameterDescriptors.ToImmutableList();
        }

        private static ParameterDescriptor CreateParameterDescriptor(MethodInfo methodInfo, ParameterInfo parameter,
            HttpMethod httpMethod)
        {
            var bindingSourceMetadata =
                parameter.GetCustomAttributes().OfType<IBindingSourceMetadata>().FirstOrDefault();
            ParameterDescriptor parameterDescriptor = null;
            if (bindingSourceMetadata != null)
            {
                var parameterFrom = bindingSourceMetadata.BindingSource.Id.To<ParameterFrom>();
                if (httpMethod == HttpMethod.Get && parameterFrom == ParameterFrom.Body)
                {
                    throw new SilkyException(
                        "Get requests are not allowed to obtain parameter values through RequestBody");
                }

                if (parameterFrom == ParameterFrom.Path && !parameter.ParameterType.IsSample())
                {
                    throw new SilkyException($"Route type parameters are not allowed to be complex data types");
                }

                parameterDescriptor = new ParameterDescriptor(parameterFrom, parameter);
            }
            else
            {
                if (parameter.IsSampleType())
                {
                    var httpMethodAttribute =
                        methodInfo.GetCustomAttributes().OfType<HttpMethodAttribute>().FirstOrDefault(p =>
                            p.HttpMethods.Contains(httpMethod.ToString().ToUpper()));
                    if (httpMethodAttribute == null)
                    {
                        parameterDescriptor =
                            new ParameterDescriptor(ParameterFrom.Query, parameter);
                    }
                    else
                    {
                        var routeTemplate = httpMethodAttribute.Template;
                        var routeTemplateSegments = routeTemplate.Split("/");
                        var parameterFromPath = false;
                        foreach (var routeTemplateSegment in routeTemplateSegments)
                        {
                            if (TemplateSegmentHelper.IsVariable(routeTemplateSegment)
                                && TemplateSegmentHelper.GetVariableName(routeTemplateSegment) == parameter.Name)
                            {
                                parameterDescriptor =
                                    new ParameterDescriptor(ParameterFrom.Path, parameter,
                                        TemplateSegmentHelper.GetSegmentVal(routeTemplateSegment));
                                parameterFromPath = true;
                                break;
                            }
                        }

                        if (!parameterFromPath)
                        {
                            parameterDescriptor =
                                new ParameterDescriptor(ParameterFrom.Query, parameter);
                        }
                    }
                }
                else
                {
                    parameterDescriptor = httpMethod == HttpMethod.Get
                        ? new ParameterDescriptor(ParameterFrom.Query, parameter)
                        : new ParameterDescriptor(ParameterFrom.Body, parameter);
                }
            }

            return parameterDescriptor;
        }
    }
}