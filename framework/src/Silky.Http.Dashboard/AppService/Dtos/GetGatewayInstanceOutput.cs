using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetGatewayInstanceOutput
    {
        public string HostName { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public ServiceProtocol ServiceProtocol { get; set; }
    }
}