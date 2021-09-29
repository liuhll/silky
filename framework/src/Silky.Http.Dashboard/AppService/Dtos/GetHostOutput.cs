using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetHostOutput
    {
        public string HostName { get; set; }

        public int InstanceCount { get; set; }

        public int AppServiceCount { get; set; }

        public int ServiceEntriesCount { get; set; }
        public ServiceProtocol[] ServiceProtocols { get; set; }
    }
}