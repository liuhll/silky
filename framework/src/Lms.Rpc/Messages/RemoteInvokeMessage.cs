using System.Collections.Generic;

namespace Lms.Rpc.Messages
{
    public class RemoteInvokeMessage : IRemoteMessage
    {
        public string ServiceId { get; set; }

        public IList<object> Parameters { get; set; }

        public IDictionary<string, object> Attachments { get; set; }
    }
}