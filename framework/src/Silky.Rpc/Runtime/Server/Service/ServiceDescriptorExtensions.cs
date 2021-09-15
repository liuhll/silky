using System.Collections.Generic;
using Silky.Core;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceDescriptorExtensions
    {
        public static string GetAuthor(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceConstant.AuthorKey);
        }

        public static string GetWsPath(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceConstant.WsPath);
        }

        public static IDictionary<string, int> GetServiceKeys(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<IDictionary<string, int>>(ServiceConstant.ServiceKey);
        }

        public static string GetHostName(this ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.Metadatas.ContainsKey(ServiceConstant.HostName))
            {
                return serviceDescriptor.GetMetadata<string>(ServiceConstant.HostName);
            }

            var serviceRouteCache = EngineContext.Current.Resolve<ServiceRouteCache>();
            var serviceRoute = serviceRouteCache.GetServiceRoute(serviceDescriptor.Id);
            if (serviceRoute != null && serviceRoute.Service.Metadatas.ContainsKey(ServiceConstant.HostName))
            {
                return serviceRoute.Service.GetMetadata<string>(ServiceConstant.HostName);
            }

            return null;
        }
    }
}