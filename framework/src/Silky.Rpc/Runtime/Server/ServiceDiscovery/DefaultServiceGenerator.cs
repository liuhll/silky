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

        public ServiceInfo CreateService((Type, bool) serviceTypeInfo)
        {
            var serviceInfo = new ServiceInfo()
            {
                Id = _serviceIdGenerator.GenerateServiceId(serviceTypeInfo.Item1),
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = ServiceProtocol.Tcp
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        public ServiceInfo CreateWsService(Type wsServiceType)
        {
            var wsPath = WebSocketResolverHelper.ParseWsPath(wsServiceType);
            var serviceId = WebSocketResolverHelper.Generator(wsPath);
            var serviceInfo = new ServiceInfo()
            {
                Id = serviceId,
                ServiceType = wsServiceType,
                IsLocal = true,
                ServiceProtocol = ServiceProtocol.Ws
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        private ServiceDescriptor CreateServiceDescriptor(ServiceInfo serviceInfo)
        {
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(serviceInfo.ServiceType);
            var serviceDescriptor = new ServiceDescriptor()
            {
                ServiceProtocol = serviceInfo.ServiceProtocol,
                Id = serviceInfo.Id,
                Service = serviceInfo.ServiceType.Name,
                Application = serviceInfo.IsLocal ? serviceBundleProvider.Application : string.Empty
            };

            if (serviceInfo.ServiceProtocol == ServiceProtocol.Tcp)
            {
                serviceDescriptor.ServiceEntries = serviceEntryManager.GetServiceEntries(serviceInfo.Id)
                    .Select(p => p.ServiceEntryDescriptor).ToArray();
            }

            var metaDataList = serviceInfo.ServiceType.GetCustomAttributes<MetadataAttribute>();
            foreach (var metaData in metaDataList)
            {
                serviceDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }

            if (serviceInfo.ServiceProtocol == ServiceProtocol.Ws)
            {
                serviceDescriptor.Metadatas.Add(ServiceConstant.WsPath,
                    WebSocketResolverHelper.ParseWsPath(serviceInfo.ServiceType));
            }

            return serviceDescriptor;
        }
    }
}