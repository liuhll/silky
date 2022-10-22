using System;
using System.Collections.Generic;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Runtime.Server
{
    public class Service
    {
        public string Id { get; set; }

        public ServiceDescriptor ServiceDescriptor { get; set; }

        public bool IsLocal { get; set; }

        public Type ServiceType { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
        
        public IReadOnlyCollection<ServiceEntry> ServiceEntries { get; set; } 
    }
}