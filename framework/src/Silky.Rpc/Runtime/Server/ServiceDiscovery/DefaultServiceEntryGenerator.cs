using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Template;
using Silky.Rpc.Runtime.Server.Descriptor;
using Silky.Rpc.Runtime.Server.Parameter;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    public class DefaultServiceEntryGenerator : IServiceEntryGenerator
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IParameterProvider _parameterProvider;
        private readonly IHttpMethodProvider _httpMethodProvider;
        private GovernanceOptions _governanceOptions;

        public DefaultServiceEntryGenerator(IServiceIdGenerator serviceIdGenerator,
            IParameterProvider parameterProvider,
            IHttpMethodProvider httpMethodProvider,
            IOptionsMonitor<GovernanceOptions> governanceOptions)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _parameterProvider = parameterProvider;
            _httpMethodProvider = httpMethodProvider;
            _governanceOptions = governanceOptions.CurrentValue;
            governanceOptions.OnChange(GovernanceOptionsChangeListener);
        }

        private void GovernanceOptionsChangeListener(GovernanceOptions options, string name)
        {
            _governanceOptions = options;
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var serviceEntryCache = EngineContext.Current.Resolve<ServiceEntryCache>();
            var serviceEntries = serviceEntryManager.GetAllEntries();
            
            foreach (var serviceEntry in serviceEntries)
            {
                serviceEntry.UpdateGovernance(options);
                serviceEntryCache.UpdateServiceEntryCache(serviceEntry);
            }
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

                        if (method.Name.StartsWith("Search"))
                        {
                            httpMethod = HttpMethod.Get;
                        }

                        if (method.Name.StartsWith("Query"))
                        {
                            httpMethod = HttpMethod.Get;
                        }

                        if (method.Name.StartsWith("Get"))
                        {
                            httpMethod = HttpMethod.Get;
                        }
                    }

                    var serviceEntryTemplate =
                        TemplateHelper.GenerateServerEntryTemplate(routeTemplate, httpMethodAttribute.Template,
                            httpMethod, isSpecify,
                            method.Name);

                    yield return Create(method,
                        serviceType.Item1,
                        serviceType.Item2,
                        serviceEntryTemplate,
                        serviceBundleProvider,
                        httpMethod);
                }
            }
        }

        private ServiceEntry Create(MethodInfo method,
            Type serviceType,
            bool isLocal,
            string serviceEntryTemplate,
            IRouteTemplateProvider routeTemplateProvider,
            HttpMethod httpMethod)
        {
            var serviceName = serviceType.Name;
            var router = new Router(serviceEntryTemplate, serviceName, method, httpMethod);
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            var parameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethod);
            if (parameterDescriptors.Count(p => p.IsHashKey) > 1)
            {
                throw new SilkyException("It is not allowed to specify multiple HashKey");
            }

            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
                ServiceProtocol = ServiceProtocol.Tcp,
            };

            var serviceEntry = new ServiceEntry(router,
                serviceDescriptor,
                serviceType,
                method,
                parameterDescriptors,
                routeTemplateProvider,
                isLocal,
                _governanceOptions);
            return serviceEntry;
        }
    }
}