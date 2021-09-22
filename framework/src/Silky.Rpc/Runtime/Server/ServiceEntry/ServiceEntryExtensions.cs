using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceEntryExtensions
    {
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

        public static IDictionary<string, object> CreateDictParameters(this ServiceEntry serviceEntry,
            [NotNull] object[] parameters)
        {
            Check.NotNull(parameters, nameof(parameters));
            var dictionaryParms = new Dictionary<string, object>();
            var index = 0;
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameter in serviceEntry.ParameterDescriptors)
            {
                if (parameter.IsSample)
                {
                    dictionaryParms[parameter.Name] = parameters[index];
                }
                else
                {
                    dictionaryParms[parameter.Name] = typeConvertibleService.Convert(parameters[index], parameter.Type);
                }

                index++;
            }

            return dictionaryParms;
        }

        public static object[] ConvertParameters(this ServiceEntry serviceEntry, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && parameters[i].GetType() != serviceEntry.ParameterDescriptors[i].Type)
                {
                    var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                    parameters[i] =
                        typeConvertibleService.Convert(parameters[i], serviceEntry.ParameterDescriptors[i].Type);
                }
            }

            return parameters;
        }

        public static bool IsTransactionServiceEntry([NotNull] this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            return serviceEntry.CustomAttributes.Any(p =>
                p.GetType().GetTypeInfo().FullName == "Silky.Transaction.TransactionAttribute");
        }

        public static ServiceDescriptor GetServiceDescriptor(this ServiceEntry serviceEntry)
        {
            var serverManager = EngineContext.Current.Resolve<IServerManager>();
            var serviceDescriptor = serverManager.GetServiceDescriptor(serviceEntry.ServiceId);
            return serviceDescriptor;
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
                    throw new SilkyException("The value specified by hashKey is not allowed to be empty");
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
                    throw new SilkyException("The value of the attribute specified by hashKey cannot be empty");
                }

                hashKey = propValue.ToString();
            }

            return hashKey;
        }
    }
}