using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryInstanceOutput
    {
        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string ServiceEntryId { get; set; }

        public string Address { get; set; }

        public bool IsHealth { get; set; }
        
        public bool Enabled { get; set; }
        
        public ServiceProtocol ServiceProtocol { get; set; }
        
    }
}