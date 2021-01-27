using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public class ParameterProvider : IParameterProvider
    {
        public IReadOnlyList<ParameterDescriptor> GetParameterDescriptors(MethodInfo methodInfo,
            HttpMethod httpMethod)
        {
            var parameterDescriptors = new List<ParameterDescriptor>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                var parameterDescriptor = CreateParameterDescriptor(parameter, httpMethod);
                if (parameterDescriptor.From == ParameterFrom.Body &&
                    parameterDescriptors.Any(p => p.From == ParameterFrom.Body))
                {
                    throw new LmsException("Request Body的参数只允许设置一个");
                }

                parameterDescriptors.Add(parameterDescriptor);
            }

            return parameterDescriptors.ToImmutableList();
        }

        private static ParameterDescriptor CreateParameterDescriptor(ParameterInfo parameter,
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
                    throw new LmsException("Get请求不允许通过RequestBody获取参数值");
                }

                parameterDescriptor = new ParameterDescriptor(parameter.Name, parameter.ParameterType, parameterFrom);
            }

            else if (httpMethod == HttpMethod.Get)
            {
                parameterDescriptor =
                    new ParameterDescriptor(parameter.Name, parameter.ParameterType, ParameterFrom.Query);
            }
            else
            {
                parameterDescriptor = parameter.IsSampleType() ? new ParameterDescriptor(parameter.Name, parameter.ParameterType, ParameterFrom.Query) : new ParameterDescriptor(parameter.Name, parameter.ParameterType, ParameterFrom.Body);
            }

            return parameterDescriptor;
        }
    }
}