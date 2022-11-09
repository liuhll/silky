using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Reflection;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Routing;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceGenerator : IServiceGenerator
    {
        private readonly IIdGenerator _idGenerator;
        private readonly ITypeFinder _typeFinder;
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServiceGenerator(IIdGenerator idGenerator,
            ITypeFinder typeFinder,
            IServiceEntryManager serviceEntryManager)
        {
            _idGenerator = idGenerator;
            _typeFinder = typeFinder;
            _serviceEntryManager = serviceEntryManager;
        }

        public Service CreateService((Type, bool) serviceTypeInfo)
        {
         
            var serviceId = _idGenerator.GenerateServiceId(serviceTypeInfo.Item1);
            var serviceInfo = new Service()
            {
                Id = serviceId,
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = ServiceHelper.GetServiceProtocol(serviceTypeInfo.Item1, serviceTypeInfo.Item2, true),
                ServiceEntries =  _serviceEntryManager.GetServiceEntries(serviceId)
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        public Service CreateWsService(Type wsServiceType)
        {
            var wsPath = WebSocketResolverHelper.ParseWsPath(wsServiceType);
            var serviceId = WebSocketResolverHelper.Generator(wsPath);
      
            var serviceInfo = new Service()
            {
                Id = serviceId,
                ServiceType = wsServiceType,
                IsLocal = true,
                ServiceProtocol = ServiceProtocol.Ws,
                ServiceEntries =  _serviceEntryManager.GetServiceEntries(serviceId)
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        private ServiceDescriptor CreateServiceDescriptor(Service service)
        {
           
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(service.ServiceType);
            var serviceDescriptor = new ServiceDescriptor
            {
                ServiceProtocol = service.ServiceProtocol,
                Id = service.Id,
                ServiceName = serviceBundleProvider.GetServiceName(service.ServiceType),
                ServiceEntries = service.ServiceEntries.Select(p => p.ServiceEntryDescriptor).ToArray()
            };

            if (service.IsLocal)
            {
                var implementTypes = ServiceHelper.FindLocalServiceImplementTypes(_typeFinder, service.ServiceType);
                var serviceKeys = new Dictionary<string, int>();
                foreach (var implementType in implementTypes)
                {
                    var serviceKeyProvider = implementType.GetCustomAttributes().OfType<IServiceKeyProvider>()
                        .FirstOrDefault();
                    if (serviceKeyProvider != null)
                    {
                        if (serviceKeys.ContainsKey(serviceKeyProvider.Name))
                        {
                            throw new SilkyException(
                                $"The {service.ServiceType.FullName} set ServiceKey is not allowed to be repeated");
                        }

                        serviceKeys.Add(serviceKeyProvider.Name, serviceKeyProvider.Weight);
                    }
                }

                if (serviceKeys.Any())
                {
                    serviceDescriptor.Metadatas.Add(ServiceConstant.ServiceKey, serviceKeys);
                }
            }

            var metaDataList = service.ServiceType.GetCustomAttributes<MetadataAttribute>();
            foreach (var metaData in metaDataList)
            {
                serviceDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }

            if (service.ServiceProtocol == ServiceProtocol.Ws)
            {
                serviceDescriptor.Metadatas.Add(ServiceConstant.WsPath,
                    WebSocketResolverHelper.ParseWsPath(service.ServiceType));
            }

            return serviceDescriptor;
        }
    }
}