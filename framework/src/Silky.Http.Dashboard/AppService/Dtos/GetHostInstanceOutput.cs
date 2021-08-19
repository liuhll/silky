using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetHostInstanceOutput
    {
        public string HostName { get; set; }

        public string Address { get; set; }

        public bool IsHealth { get; set; }

        public bool IsEnable { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}