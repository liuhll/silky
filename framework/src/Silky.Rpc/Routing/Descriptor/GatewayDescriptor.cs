using System;
using System.Collections.Generic;
using Silky.Core.Utils;

namespace Silky.Rpc.Routing.Descriptor
{
    public class GatewayDescriptor
    {
        public GatewayDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public string HostName { get; set; }
        
        public IEnumerable<string> Addresses { get; set; } = new List<string>();
        
        public long TimeStamp { get; set; }
    }
}