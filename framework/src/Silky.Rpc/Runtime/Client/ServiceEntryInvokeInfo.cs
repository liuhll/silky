using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Client
{
    public class ServiceEntryInvokeInfo
    {
        public ServiceInvokeInfo ServiceInvokeInfo { get; set; }

        public IList<string> Addresses { get; set; }

        public string ServiceId { get; set; }
    }
}