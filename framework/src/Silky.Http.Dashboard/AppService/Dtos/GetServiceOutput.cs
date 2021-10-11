using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceOutput
    {
        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}