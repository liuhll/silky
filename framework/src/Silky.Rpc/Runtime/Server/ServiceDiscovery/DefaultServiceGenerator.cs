using System;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    public class DefaultServiceGenerator : IServiceGenerator
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;

        public DefaultServiceGenerator(IServiceIdGenerator serviceIdGenerator)
        {
            _serviceIdGenerator = serviceIdGenerator;
        }

        public Service CreateService((Type, bool) serviceTypeInfo)
        {
            var serviceInfo = new Service()
            {
                Id = _serviceIdGenerator.GenerateServiceId(serviceTypeInfo.Item1),
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = ServiceProtocol.Tcp
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
                Service = service.ServiceType.Name,
                Application = service.IsLocal ? serviceBundleProvider.Application : string.Empty
            };

            if (service.ServiceProtocol == ServiceProtocol.Tcp)
            {
                serviceDescriptor.ServiceEntries = serviceEntryManager.GetServiceEntries(service.Id)
                    .Select(p => p.ServiceEntryDescriptor).ToArray();
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