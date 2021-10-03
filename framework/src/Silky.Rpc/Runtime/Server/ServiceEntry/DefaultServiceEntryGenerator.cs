using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Silky.Core.Exceptions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceEntryGenerator : IServiceEntryGenerator
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IParameterProvider _parameterProvider;
        private GovernanceOptions _governanceOptions;


        public DefaultServiceEntryGenerator(IIdGenerator idGenerator,
            IParameterProvider parameterProvider,
            IOptionsMonitor<GovernanceOptions> governanceOptions,
            ITypeFinder typeFinder)
        {
            _idGenerator = idGenerator;
            _parameterProvider = parameterProvider;
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
                var httpMethodInfos = method.GetHttpMethodInfos();
                foreach (var httpMethodInfo in httpMethodInfos)
                {
                    var serviceEntryTemplate =
                        TemplateHelper.GenerateServerEntryTemplate(routeTemplate, httpMethodInfo.Template,
                            httpMethodInfo.HttpMethod, httpMethodInfo.IsSpecify,
                            method.Name);
                    yield return Create(method,
                        serviceType.Item1,
                        serviceType.Item2,
                        serviceEntryTemplate,
                        serviceBundleProvider,
                        httpMethodInfo.HttpMethod);
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
            var serviceEntryId = _idGenerator.GenerateServiceEntryId(method, httpMethod);
            var serviceId = _idGenerator.GenerateServiceId(serviceType);
            var parameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethod);
            if (parameterDescriptors.Count(p => p.IsHashKey) > 1)
            {
                throw new SilkyException(
                    $"It is not allowed to specify multiple HashKey,Method is {serviceType.FullName}.{method.Name}");
            }

            Debug.Assert(method.DeclaringType != null);
            var serviceEntryDescriptor = new ServiceEntryDescriptor()
            {
                Id = serviceEntryId,
                ServiceId = serviceId,
                ServiceName = routeTemplateProvider.GetServiceName(serviceType),
                ServiceProtocol = ServiceHelper.GetServiceProtocol(serviceType, isLocal, false),
                Method = method.Name,
            };

            var metaDataList = method.GetCustomAttributes<MetadataAttribute>();

            foreach (var metaData in metaDataList)
            {
                serviceEntryDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }

            var serviceEntry = new ServiceEntry(router,
                serviceEntryDescriptor,
                serviceType,
                method,
                parameterDescriptors,
                isLocal,
                _governanceOptions);
            return serviceEntry;
        }
    }
}