using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Rpc.Ids;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IRoutePathParser _routePathParser;


        public ClrServiceEntryFactory(IServiceIdGenerator serviceIdGenerator,
            IRoutePathParser routePathParser)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _routePathParser = routePathParser;
        }

        public IEnumerable<ServiceEntry> CreateServiceEntry(Type serviceType)
        {
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(serviceType);
            var routeTemplate = serviceBundleProvider.RouteTemplate;

            foreach (var method in serviceType.GetMethods())
            {
                var routeIsReWriteByServiceRoute = false;
                var serviceRouteProvider = ServiceDiscoveryHelper.GetServiceRouteProvider(method);
                if (serviceRouteProvider != null)
                {
                    routeIsReWriteByServiceRoute = true;
                    if (serviceBundleProvider.IsPrefix)
                    {
                        var prefixRouteTemplate = routeTemplate;
                        if (prefixRouteTemplate.Contains("{method}", StringComparison.OrdinalIgnoreCase))
                        {
                            prefixRouteTemplate = prefixRouteTemplate
                                .Replace("{method}", "", StringComparison.OrdinalIgnoreCase).TrimEnd('/');
                        }

                        routeTemplate = $"{prefixRouteTemplate}/{serviceRouteProvider.Template}";
                    }
                }

                yield return Create(method, serviceType.Name, routeTemplate, routeIsReWriteByServiceRoute);
            }
        }

        private ServiceEntry Create(MethodInfo method, string serviceName, string routeTemplate,
            bool routeIsReWriteByServiceRoute = false)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
                RoutePath = _routePathParser.Parse(routeTemplate, serviceName, method.Name,
                    routeIsReWriteByServiceRoute)
            };
            var fastInvoker = GetHandler(serviceId, method);
            return new ServiceEntry()
            {
                Descriptor = serviceDescriptor,
                Func = (key, parameters) =>
                {
                    object instance = EngineContext.Current.Resolve(method.DeclaringType);
                    var list = new List<object>();
                    foreach (var parameterInfo in method.GetParameters())
                    {
                        if (parameters.ContainsKey(parameterInfo.Name))
                        {
                            var value = parameters[parameterInfo.Name];
                            var parameterType = parameterInfo.ParameterType;
                            // var parameter = _typeConvertibleService.Convert(value, parameterType);
                            // list.Add(parameter);
                        }
                        //加入是否有默认值的判断，有默认值，并且用户没传，取默认值
                        else if (parameterInfo.HasDefaultValue && !parameters.ContainsKey(parameterInfo.Name))
                        {
                            list.Add(parameterInfo.DefaultValue);
                        }
                        else
                        {
                            list.Add(null);
                        }
                    }

                    return Task.FromResult(fastInvoker(instance, list.ToArray()));

                }
            };
        }

        private FastInvokeHandler GetHandler(string serviceId, MethodInfo method)
        {
            return FastInvoke.GetMethodInvoker(method);
        }
    }
}