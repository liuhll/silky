using Silky.Core.Rpc;

namespace Silky.HealthChecks.Rpc
{
    public class ServerHealthData
    {
        public string HostName { get; set; }

        public string Address { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public bool Health { get; set; }
    }
}