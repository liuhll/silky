using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Silky.Core.Exceptions;
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
            IOptionsMonitor<GovernanceOptions> governanceOptions)
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
            var methods = serviceType.Item1.GetTypeInfo().GetMethods();
            foreach (var method in methods)
            {
                var httpMethodInfos = method.GetHttpMethodInfos();
                foreach (var httpMethodInfo in httpMethodInfos)
                {
                    yield return Create(method,
                        serviceType.Item1,
                        serviceType.Item2,
                        serviceBundleProvider,
                        httpMethodInfo);
                }
            }
        }

        private ServiceEntry Create(MethodInfo method,
            Type serviceType,
            bool isLocal,
            IRouteTemplateProvider routeTemplateProvider,
            HttpMethodInfo httpMethodInfo)
        {
            var serviceName = serviceType.Name;
            var serviceEntryId = _idGenerator.GenerateServiceEntryId(method, httpMethodInfo.HttpMethod);
            var serviceId = _idGenerator.GenerateServiceId(serviceType);
            var parameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethodInfo);
            if (parameterDescriptors.Count(p => p.IsHashKey) > 1)
            {
                throw new SilkyException(
                    $"It is not allowed to specify multiple HashKey,Method is {serviceType.FullName}.{method.Name}");
            }
            
            var serviceEntryTemplate =
                TemplateHelper.GenerateServerEntryTemplate(routeTemplateProvider.Template, parameterDescriptors,
                    httpMethodInfo, _governanceOptions.ApiIsRESTfulStyle,
                    method.Name);

            var router = new Router(serviceEntryTemplate, serviceName, method, httpMethodInfo.HttpMethod);
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

            if (serviceEntry.NeedHttpProtocolSupport())
            {
                serviceEntryDescriptor.Metadatas.Add(ServiceEntryConstant.NeedHttpProtocolSupport, true);
            }

            if (serviceEntry.IsSilkyAppService())
            {
                serviceEntryDescriptor.Metadatas.Add(ServiceEntryConstant.IsSilkyAppService, true);
            }
            
            return serviceEntry;
        }
    }
}