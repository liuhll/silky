using System.Collections.Generic;
using System.Linq;
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


        public static bool HasMetaData(this ServiceDescriptor serviceDescriptor, string key)
        {
            return serviceDescriptor.Metadatas.ContainsKey(key);
        }

        public static bool IsSilkyService(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.HasMetaData(ServiceConstant.IsSilkyService) &&
                   serviceDescriptor.GetMetadata<bool>(ServiceConstant.IsSilkyService);
        }

        public static bool MultiServiceKeys(this ServiceDescriptor serviceDescriptor)
        {
            var serviceKeys = serviceDescriptor.GetServiceKeys();
            return serviceKeys != null && serviceKeys.Any();
        }
    }
}