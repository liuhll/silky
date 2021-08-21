using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryRouteOutput
    {
        public string ServiceId { get; set; }
        

        public string Address { get; set; }

        public bool IsHealth { get; set; }

        public bool IsEnable { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}