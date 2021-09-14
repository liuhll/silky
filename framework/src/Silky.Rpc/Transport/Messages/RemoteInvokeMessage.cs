using System.Collections.Generic;

namespace Silky.Rpc.Transport.Messages
{
    public class RemoteInvokeMessage : IRemoteMessage
    {
        public string ServiceEntryId { get; set; }

        public string ServiceId { get; set; }

        public object[] Parameters { get; set; }

        public IDictionary<string, object> Attachments { get; set; }
    }
}