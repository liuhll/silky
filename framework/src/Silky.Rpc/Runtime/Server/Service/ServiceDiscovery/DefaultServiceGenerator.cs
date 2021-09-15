using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Rpc.Routing;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceGenerator : IServiceGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public DefaultServiceGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public Service CreateService((Type, bool) serviceTypeInfo)
        {
            var serviceInfo = new Service()
            {
                Id = _idGenerator.GenerateServiceId(serviceTypeInfo.Item1),
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = EngineContext.Current.IsContainHttpCoreModule() ? ServiceProtocol.Http : ServiceProtocol.Tcp
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
                Application = serviceBundleProvider.Application
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