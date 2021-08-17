using System;
using System.Collections.Generic;
using Silky.Core.Utils;
using Silky.Rpc.Address.Descriptor;

namespace Silky.Rpc.Gateway.Descriptor
{
    public class GatewayDescriptor
    {
        public GatewayDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public string HostName { get; set; }

        public IEnumerable<AddressDescriptor> Addresses { get; set; } = new List<AddressDescriptor>();

        public IEnumerable<string> SupportServices { get; set; } = new List<string>();

        public long TimeStamp { get; set; }
    }
}