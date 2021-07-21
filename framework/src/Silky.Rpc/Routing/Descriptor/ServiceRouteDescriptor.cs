using System;
using System.Collections.Generic;
using Silky.Core.Utils;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public class ServiceRouteDescriptor
    {
        public ServiceRouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public ServiceDescriptor ServiceDescriptor { get; set; }

        public IEnumerable<AddressDescriptor> AddressDescriptors { get; set; } = new List<AddressDescriptor>();

        public long TimeStamp { get; set; }
        
    }
}