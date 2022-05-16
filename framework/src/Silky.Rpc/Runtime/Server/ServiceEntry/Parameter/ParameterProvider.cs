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
            HttpMethodInfo httpMethodInfo)
        {
            var parameterDescriptors = new List<ParameterDescriptor>();
            var index = 0;
            foreach (var parameter in methodInfo.GetParameters())
            {
                var parameterDescriptor = CreateParameterDescriptor(methodInfo, parameter, httpMethodInfo, index);
                if (parameterDescriptor.From == ParameterFrom.Body &&
                    parameterDescriptors.Any(p => p.From == ParameterFrom.Body))
                {
                    throw new SilkyException("Only one parameter of Request Body is allowed to be set");
                }

                parameterDescriptors.Add(parameterDescriptor);
                index += 1;
            }

            return parameterDescriptors.ToImmutableList();
        }

        private ParameterDescriptor CreateParameterDescriptor(MethodInfo methodInfo, ParameterInfo parameter,
            HttpMethodInfo httpMethodInfo, int index)
        {
            var bindingSourceMetadata =
                parameter.GetCustomAttributes().OfType<IBindingSourceMetadata>().FirstOrDefault();
            ParameterDescriptor parameterDescriptor = null;
            if (bindingSourceMetadata != null)
            {
                var parameterFrom = bindingSourceMetadata.BindingSource.Id.To<ParameterFrom>();
                if (httpMethodInfo.HttpMethod == HttpMethod.Get && parameterFrom == ParameterFrom.Body)
                {
                    throw new SilkyException(
                        "Get requests are not allowed to obtain parameter values through RequestBody");
                }

                if (parameterFrom == ParameterFrom.Path && !parameter.ParameterType.IsSample())
                {
                    throw new SilkyException($"Route type parameters are not allowed to be complex data types");
                }

                parameterDescriptor = new ParameterDescriptor(parameterFrom, parameter, index);
            }
            else
            {
                if (parameter.IsSampleOrNullableType())
                {
                    var httpMethodAttribute =
                        methodInfo.GetCustomAttributes().OfType<HttpMethodAttribute>().FirstOrDefault(p =>
                            p.HttpMethods.Contains(httpMethodInfo.HttpMethod.ToString().ToUpper()));
                    if (httpMethodAttribute == null)
                    {
                        parameterDescriptor = parameter.IsSampleType()
                            ? new ParameterDescriptor(ParameterFrom.Path, parameter, index)
                            : new ParameterDescriptor(ParameterFrom.Query, parameter, index);
                    }
                    else
                    {
                        var httpRouteTemplate = httpMethodAttribute.Template;
                        var parameterFromPath = false;
                        if (!httpRouteTemplate.IsNullOrWhiteSpace())
                        {
                            var routeTemplateSegments = httpRouteTemplate.Split("/");
                            foreach (var routeTemplateSegment in routeTemplateSegments)
                            {
                                if (TemplateSegmentHelper.IsVariable(routeTemplateSegment)
                                    && TemplateSegmentHelper.GetVariableName(routeTemplateSegment) == parameter.Name)
                                {
                                    parameterDescriptor =
                                        new ParameterDescriptor(ParameterFrom.Path, parameter,
                                            index,
                                            TemplateSegmentHelper.GetSegmentVal(routeTemplateSegment),
                                            routeTemplateSegment);
                                    parameterFromPath = true;
                                    break;
                                }
                            }

                            if (!parameterFromPath)
                            {
                                parameterDescriptor = new ParameterDescriptor(ParameterFrom.Query, parameter, index);
                            }
                        }
                        else
                        {
                            parameterDescriptor = parameter.IsSampleType()
                                ? new ParameterDescriptor(ParameterFrom.Path, parameter, index)
                                : new ParameterDescriptor(ParameterFrom.Query, parameter, index);
                        }
                    }
                }
                else if (parameter.IsFormFileType())
                {
                    parameterDescriptor = new ParameterDescriptor(ParameterFrom.Form, parameter, index);
                }
                else
                {
                    parameterDescriptor = httpMethodInfo.HttpMethod == HttpMethod.Get
                        ? new ParameterDescriptor(ParameterFrom.Query, parameter, index)
                        : new ParameterDescriptor(ParameterFrom.Body, parameter, index);
                }
            }

            return parameterDescriptor;
        }
    }
}