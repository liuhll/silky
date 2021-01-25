using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.Rpc.Ids;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IRoutePathParser _routePathParser;
        private readonly IParameterProvider _parameterProvider;
        private readonly IHttpMethodProvider _httpMethodProvider;

        public ClrServiceEntryFactory(IServiceIdGenerator serviceIdGenerator,
            IRoutePathParser routePathParser, 
            IParameterProvider parameterProvider, 
            IHttpMethodProvider httpMethodProvider)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _routePathParser = routePathParser;
            _parameterProvider = parameterProvider;
            _httpMethodProvider = httpMethodProvider;
        }

        public IEnumerable<ServiceEntry> CreateServiceEntry(Type serviceType)
        {
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(serviceType);
            var routeTemplate = serviceBundleProvider.RouteTemplate;
            var methods = serviceType.GetTypeInfo().GetMethods();
            
            foreach (var method in methods)
            {
                var routeIsReWriteByServiceRoute = false;
                
                var httpMethods = _httpMethodProvider.GetHttpMethods(method);

                foreach (var httpMethod in httpMethods)
                {
                    var serviceEntryTemplate = httpMethod.Template;
                    if (!serviceEntryTemplate.IsNullOrEmpty())
                    {
                        routeIsReWriteByServiceRoute = true;
                        if (serviceBundleProvider.IsPrefix)
                        {
                            var prefixRouteTemplate = routeTemplate;
                            if (prefixRouteTemplate.Contains("{method}", StringComparison.OrdinalIgnoreCase))
                            {
                                prefixRouteTemplate = prefixRouteTemplate .Replace("{method}", "", StringComparison.OrdinalIgnoreCase).TrimEnd('/');
                            }
                            routeTemplate = $"{prefixRouteTemplate}/{serviceEntryTemplate}";
                        }
                    }
                    
                    yield return Create(method, serviceType.Name, routeTemplate, httpMethod, routeIsReWriteByServiceRoute);
                }
                
            }
        }

        private ServiceEntry Create(MethodInfo method, string serviceName, string routeTemplate,
            HttpMethodAttribute httpMethod,
            bool routeIsReWriteByServiceRoute = false)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method,httpMethod);
            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
               // RoutePath = _routePathParser.Parse(routeTemplate, serviceName, method.Name,
               //     routeIsReWriteByServiceRoute)
            };
            var fastInvoker = GetHandler(serviceId, method);
            var serviceEntry = new ServiceEntry()
            {
                ServiceDescriptor = serviceDescriptor,
                ParameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethod),
                Func = (key, parameters) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        object instance = EngineContext.Current.Resolve(method.DeclaringType);
                        var list = new List<object>();
                        foreach (var parameter in parameters)
                        {
                            switch (parameter.Key)
                            {
                                
                            }
                        }

                        return fastInvoker(instance, list.ToArray());
                    });

                }
            };
            return serviceEntry;
        }

        private FastInvokeHandler GetHandler(string serviceId, MethodInfo method)
        {
            return FastInvoke.GetMethodInvoker(method);
        }
    }
}