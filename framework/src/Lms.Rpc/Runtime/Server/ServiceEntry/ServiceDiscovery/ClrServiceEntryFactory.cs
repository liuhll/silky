using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.Core.MethodExecutor;
using Lms.Rpc.Ids;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Template;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IParameterProvider _parameterProvider;
        private readonly IHttpMethodProvider _httpMethodProvider;

        public ClrServiceEntryFactory(IServiceIdGenerator serviceIdGenerator,
            IParameterProvider parameterProvider,
            IHttpMethodProvider httpMethodProvider)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _parameterProvider = parameterProvider;
            _httpMethodProvider = httpMethodProvider;
        }

        public IEnumerable<ServiceEntry> CreateServiceEntry((Type, bool) serviceType)
        {
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(serviceType.Item1);
            var routeTemplate = serviceBundleProvider.Template;
            var methods = serviceType.Item1.GetTypeInfo().GetMethods();

            foreach (var method in methods)
            {
                var (httpMethods, isSpecify) = _httpMethodProvider.GetHttpMethodsInfo(method);
                foreach (var httpMethodAttribute in httpMethods)
                {
                    var httpMethod = httpMethodAttribute.HttpMethods.First().To<HttpMethod>();
                    if (!isSpecify)
                    {
                        if (method.Name.StartsWith("Create"))
                        {
                            httpMethod = HttpMethod.Post;
                        }

                        if (method.Name.StartsWith("Update"))
                        {
                            httpMethod = HttpMethod.Put;
                        }

                        if (method.Name.StartsWith("Delete"))
                        {
                            httpMethod = HttpMethod.Delete;
                        }
                    }

                    var serviceEntryTemplate =
                        TemplateHelper.GenerateServerEntryTemplate(routeTemplate, httpMethodAttribute.Template,
                            httpMethod, isSpecify,
                            method.Name);

                    yield return Create(method, serviceType.Item1, serviceType.Item2, serviceEntryTemplate,
                        httpMethod);
                }
            }
        }

        private ServiceEntry Create(MethodInfo method, Type serviceType, bool isLocal, string serviceEntryTemplate,
            HttpMethod httpMethod)
        {
            var serviceName = serviceType.Name;
            var router = new Router(serviceEntryTemplate, serviceName, method, httpMethod);
            var serviceId = _serviceIdGenerator.GenerateServiceId(method, httpMethod);
            var parameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethod);
            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
            };
            var serviceEntry = new ServiceEntry(router, serviceDescriptor, serviceType, method, parameterDescriptors, isLocal);
            return serviceEntry;
        }

        private FastInvokeHandler GetHandler(string serviceId, MethodInfo method)
        {
            return FastInvoke.GetMethodInvoker(method);
        }
    }
}