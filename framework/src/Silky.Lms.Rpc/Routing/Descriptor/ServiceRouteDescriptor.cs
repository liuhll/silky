using System;
using System.Collections.Generic;
using Silky.Lms.Core.Utils;
using Silky.Lms.Rpc.Address.Descriptor;
using Silky.Lms.Rpc.Runtime.Server.Descriptor;

namespace Silky.Lms.Rpc.Routing.Descriptor
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