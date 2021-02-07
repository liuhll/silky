using System;
using System.Collections.Generic;
using Lms.Core.Utils;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Runtime.Server.Descriptor;

namespace Lms.Rpc.Routing.Descriptor
{
    public class ServiceRouteDescriptor
    {
        public ServiceRouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public ServiceDescriptor ServiceDescriptor { get; set; }

        public IEnumerable<AddressDescriptor> AddressDescriptors { get; set; }

        public long TimeStamp { get; set; }
        
    }
}