using System.Linq;
using System.Reflection;
using Lms.Core;
using Lms.Core.Convertible;
using Lms.Core.Exceptions;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Utils;

namespace Lms.Rpc.Runtime.Server
{
    public static class ServiceEntryExtensions
    {
        public static ServiceRouteDescriptor CreateLocalRouteDescriptor(this ServiceEntry serviceEntry)
        {
            if (!serviceEntry.IsLocal)
            {
                throw new LmsException("只允许本地服务条目生成路由描述符");
            }

            return new ServiceRouteDescriptor()
            {
                ServiceDescriptor = serviceEntry.ServiceDescriptor,
                AddressDescriptors = new[]
                    {NetUtil.GetHostAddress(serviceEntry.ServiceDescriptor.ServiceProtocol).Descriptor},
            };
        }

        public static string GetHashKeyValue(this ServiceEntry serviceEntry, object[] parameterValues)
        {
            var hashKey = string.Empty;
            if (!serviceEntry.ParameterDescriptors.Any())
            {
                hashKey = serviceEntry.Router.RoutePath + serviceEntry.Router.HttpMethod;
            }

            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            if (serviceEntry.ParameterDescriptors.Any(p => p.IsHashKey))
            {
                var index = 0;
                foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
                {
                    if (parameterDescriptor.IsHashKey)
                    {
                        hashKey = GetHashKey(parameterValues, parameterDescriptor, index, typeConvertibleService);
                        break;
                    }

                    index++;
                }
            }
            else
            {
                var index = 0;
                foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
                {
                    hashKey = GetHashKey(parameterValues, parameterDescriptor, index, typeConvertibleService);
                    break;
                }
            }

            return hashKey;
        }

        private static string GetHashKey(object[] parameterValues, ParameterDescriptor parameterDescriptor, int index,
            ITypeConvertibleService typeConvertibleService)
        {
            string hashKey;
            if (parameterDescriptor.IsSample)
            {
                var propVal = parameterValues[index];
                if (propVal == null)
                {
                    throw new LmsException("hashKey指定的值不允许为空");
                }

                hashKey = propVal.ToString();
            }
            else
            {
                var parameterValue =
                    typeConvertibleService.Convert(parameterValues[index], parameterDescriptor.Type);
                var hashKeyProp = parameterDescriptor.Type
                    .GetProperties().First();
                var hashKeyProviderProps = parameterDescriptor.Type
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes().OfType<IHashKeyProvider>().Any());
                if (hashKeyProviderProps.Any())
                {
                    hashKeyProp = hashKeyProviderProps.First();
                }

                var propValue = hashKeyProp.GetValue(parameterValue);
                if (propValue == null)
                {
                    throw new LmsException("hashKey指定的属性的值不允许为空");
                }

                hashKey = propValue.ToString();
            }

            return hashKey;
        }
    }
}