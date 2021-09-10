using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silky.Core.Utils;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public class ServiceRouteDescriptor
    {
        public ServiceRouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public ServiceDescriptor Service { get; set; }
        
        public IEnumerable<AddressDescriptor> Addresses { get; set; } = new List<AddressDescriptor>();

        public long TimeStamp { get; set; }
        
    }
}