using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryInstanceOutput
    {
        public string ServiceEntryId { get; set; }

        public string Address { get; set; }

        public bool IsHealth { get; set; }

        public bool Enabled { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
    }
}