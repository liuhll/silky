using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryHandleInfo
    {
        public string ServiceId { get; set; }
        
        public IList<string> Addresses{ get; set; }
        
        public ServiceHandleInfo ServiceHandleInfo { get; set; }
    }
}