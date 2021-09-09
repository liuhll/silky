using System;
using System.Linq;
using System.Reflection;
using Silky.Core;

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
                ServiceId = _serviceIdGenerator.GenerateServiceId(serviceTypeInfo.Item1),
                ServiceType = serviceTypeInfo.Item1,
                IsLocal = serviceTypeInfo.Item2,
                ServiceProtocol = ServiceProtocol.Tcp
            };
            serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
            return serviceInfo;
        }

        private ServiceDescriptor CreateServiceDescriptor(ServiceInfo serviceInfo)
        {
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var serviceDescriptor = new ServiceDescriptor()
            {
                ServiceProtocol = serviceInfo.ServiceProtocol,
                Id = serviceInfo.ServiceId,
                AppService = serviceInfo.ServiceType.Name,
                HostName = EngineContext.Current.HostName,
               ServiceEntries = serviceEntryManager.GetServiceEntries(serviceInfo.ServiceId).Select(p=> p.ServiceEntryDescriptor).ToArray(),
            };
            var metaDatas = serviceInfo.ServiceType.GetCustomAttributes<MetadataAttribute>();
            foreach (var metaData in metaDatas)
            {
                serviceDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }
            return serviceDescriptor;
        }
        
    }
}