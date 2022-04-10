using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec.Message
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

        public ParameterItem(KeyValuePair<ParameterFrom, object> item)
        {
            Key = item.Key.ToString();
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
            ServiceEntryId = remoteInvokeMessage.ServiceEntryId;
            Parameters = remoteInvokeMessage.Parameters?.Select(i => new DynamicItem(i)).ToArray();
            ParameterType = remoteInvokeMessage.ParameterType;
            DictParameters = remoteInvokeMessage.DictParameters?.Select(i => new ParameterItem(i)).ToArray();
            HttpParameters = remoteInvokeMessage.HttpParameters?.Select(i => new ParameterItem(i)).ToArray();
            Attachments = remoteInvokeMessage.Attachments?.Select(i => new ParameterItem(i)).ToArray();
        }

        public ProtoBufRemoteInvokeMessage()
        {
        }


        [ProtoMember(1)] public string ServiceEntryId { get; set; }

        [ProtoMember(2)] public string ServiceId { get; set; }

        [ProtoMember(3)] public DynamicItem[] Parameters { get; set; }

        [ProtoMember(4)] public ParameterItem[] DictParameters { get; set; }

        [ProtoMember(5)] public ParameterItem[] HttpParameters { get; set; }

        [ProtoMember(6)] public ParameterType ParameterType { get; set; }

        [ProtoMember(7)] public ParameterItem[] Attachments { get; set; }

        public RemoteInvokeMessage GetMessage()
        {
            return new()
            {
                ServiceEntryId = ServiceEntryId,
                Parameters = Parameters?.Select(i => i.Get()).ToArray(),
                ParameterType = ParameterType,
                DictParameters = DictParameters?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                HttpParameters =
                    HttpParameters?.ToDictionary(i => i.Key.ConventTo<ParameterFrom>(), i => i.Value?.Get()),
                Attachments = Attachments?.ToDictionary(i => i.Key, i => i.Value?.Get()),
            };
        }
    }
}