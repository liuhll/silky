using System.Collections.Generic;
using System.Linq;
using Silky.Lms.Rpc.Messages;
using ProtoBuf;

namespace Silky.Lms.Codec.Message
{
    [ProtoContract]
    public class ParameterItem
    {
        #region Constructor

        public ParameterItem(KeyValuePair<string, object> item)
        {
            Key = item.Key;
            Value = item.Value == null ? null : new DynamicItem(item.Value);
        }

        public ParameterItem()
        {
        }

        #endregion Constructor

        [ProtoMember(1)] public string Key { get; set; }

        [ProtoMember(2)] public DynamicItem Value { get; set; }
    }

    [ProtoContract]
    public class ProtoBufRemoteInvokeMessage
    {
        public ProtoBufRemoteInvokeMessage(RemoteInvokeMessage remoteInvokeMessage)
        {
            ServiceId = remoteInvokeMessage.ServiceId;
            Parameters = remoteInvokeMessage.Parameters?.Select(i => new DynamicItem(i)).ToArray();
            Attachments = remoteInvokeMessage.Attachments?.Select(i => new ParameterItem(i)).ToArray();
        }

        public ProtoBufRemoteInvokeMessage()
        {
        }


        [ProtoMember(1)] public string ServiceId { get; set; }

        [ProtoMember(2)] public DynamicItem[] Parameters { get; set; }

        [ProtoMember(3)] public ParameterItem[] Attachments { get; set; }

        public RemoteInvokeMessage GetMessage()
        {
            return new()
            {
                ServiceId = ServiceId,
                Parameters = Parameters?.Select(i => i.Get()).ToArray(),
                Attachments = Attachments?.ToDictionary(i => i.Key, i => i.Value?.Get()),
            };
        }
    }
}