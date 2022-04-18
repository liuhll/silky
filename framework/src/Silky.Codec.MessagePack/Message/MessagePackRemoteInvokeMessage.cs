using System.Collections.Generic;
using MessagePack;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec.Message
{
    [MessagePackObject]
    public class MessagePackRemoteInvokeMessage
    {
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage remoteInvokeMessage)
        {
            ServiceId = remoteInvokeMessage.ServiceId;
            ServiceEntryId = remoteInvokeMessage.ServiceEntryId;
            Parameters = remoteInvokeMessage.Parameters;
            ParameterType = remoteInvokeMessage.ParameterType;
            DictParameters = remoteInvokeMessage.DictParameters;
            HttpParameters = remoteInvokeMessage.HttpParameters;
            Attachments = remoteInvokeMessage.Attachments;
        }

        public MessagePackRemoteInvokeMessage()
        {
        }

        [Key(1)] public string ServiceEntryId { get; set; }

        [Key(2)] public string ServiceId { get; set; }

        [Key(3)] public object[] Parameters { get; set; }

        [Key(4)] public IDictionary<string, object> DictParameters { get; set; }

        [Key(5)] public IDictionary<ParameterFrom, object> HttpParameters { get; set; }

        [Key(6)] public ParameterType ParameterType { get; set; }

        [Key(7)] public IDictionary<string, object> Attachments { get; set; }

        public RemoteInvokeMessage GetMessage()
        {
            return new()
            {
                ServiceId = ServiceId,
                ServiceEntryId = ServiceEntryId,
                Parameters = Parameters,
                ParameterType = ParameterType,
                DictParameters = DictParameters,
                HttpParameters = HttpParameters,
                Attachments = Attachments,
            };
        }
    }
}