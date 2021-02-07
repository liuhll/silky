using System.Collections.Generic;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Messages
{
    public class RemoteInvokeMessage
    {
        public string ServiceId { get; set; }
        
        public IDictionary<ParameterFrom, object> Parameters { get; set; }
        
        public IDictionary<string, object> Attachments { get; set; }

        // public IDictionary<string, object> RequestHeaders { get; set; }
    }
}