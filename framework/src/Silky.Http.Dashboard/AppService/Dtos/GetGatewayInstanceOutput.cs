using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetGatewayInstanceOutput
    {
        public string HostName { get; set; }

        public string Endpoint { get; set; }

        public IList<AddressOutput> Addresses { get; set; }

    }

    public class AddressOutput
    {
        public string Address { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}