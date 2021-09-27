using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Rpc.Routing;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceGenerator : IServiceGenerator
    {
        private readonly IIdGenerator _idGenerator;
        private readonly ITypeFinder _typeFinder;

        public DefaultServiceGenerator(IIdGenerator idGenerator,
            ITypeFinder typeFinder)
        {
            _idGenerator = idGenerator;
            _typeFinder = typeFinder;
        }

        public Service CreateService((Type, bool) serviceTypeInfo)
        {
            var serviceInfo = new Service()
            {
                Id = _idGenerator.GenerateServiceId(serviceTypeInfo.Item1),
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = ServiceHelper.GetServiceProtocol(serviceTypeInfo.Item1, serviceTypeInfo.Item2, true)
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
                ServiceProtocol = ServiceProtocol.Ws
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        private ServiceDescriptor CreateServiceDescriptor(Service service)
        {
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(service.ServiceType);
            var serviceDescriptor = new ServiceDescriptor()
            {
                ServiceProtocol = service.ServiceProtocol,
                Id = service.Id,
                ServiceName = serviceBundleProvider.GetServiceName(service.ServiceType),
            };

            serviceDescriptor.ServiceEntries = serviceEntryManager.GetServiceEntries(service.Id)
                .Select(p => p.ServiceEntryDescriptor).ToArray();

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