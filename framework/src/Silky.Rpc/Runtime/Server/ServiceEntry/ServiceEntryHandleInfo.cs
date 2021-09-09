using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryHandleInfo
    {
        public string ServiceId { get; set; }
        
        public string Address{ get; set; }
        
        public ServiceHandleInfo ServiceHandleInfo { get; set; }
    }
}