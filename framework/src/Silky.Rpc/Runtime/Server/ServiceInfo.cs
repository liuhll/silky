using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceInfo
    {
        public string Id { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }

        public bool IsLocal { get; set; }

        public Type ServiceType { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}