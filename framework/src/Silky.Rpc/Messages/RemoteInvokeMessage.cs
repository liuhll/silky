using System.Collections.Generic;

namespace Silky.Rpc.Messages
{
    public class RemoteInvokeMessage : IRemoteMessage
    {
        public string ServiceId { get; set; }

        public object[] Parameters { get; set; }

        public IDictionary<string, object> Attachments { get; set; }
    }
}