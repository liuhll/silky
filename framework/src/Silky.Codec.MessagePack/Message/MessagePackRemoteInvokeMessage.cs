using System.Collections.Generic;
using Silky.Rpc.Messages;
using MessagePack;

namespace Silky.Codec.Message
{
    [MessagePackObject]
    public class MessagePackRemoteInvokeMessage
    {
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage remoteInvokeMessage)
        {
            ServiceId = remoteInvokeMessage.ServiceId;
            Parameters = remoteInvokeMessage.Parameters;
            Attachments = remoteInvokeMessage.Attachments;
        }

        public MessagePackRemoteInvokeMessage()
        {
        }

        [Key(1)]
        public string ServiceId { get; set; }

        [Key(2)]
        public object[] Parameters { get; set; }

        [Key(3)]
        public IDictionary<string, object> Attachments { get; set; }

        public RemoteInvokeMessage GetMessage()
        {
            return new()
            {
                ServiceId = ServiceId,
                Parameters = Parameters,
                Attachments = Attachments
            };
        }
    }
}