using System;
using ProtoBuf;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec.Message
{
    [ProtoContract]
    public class ProtoBufTransportMessage
    {
        public ProtoBufTransportMessage()
        {
        }

        public ProtoBufTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;
            if (transportMessage.IsInvokeMessage())
            {
                Content = SerializerUtilitys.Serialize(
                    new ProtoBufRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>()));
            }

            if (transportMessage.IsResultMessage())
            {
                Content = SerializerUtilitys.Serialize(
                    new ProtoBufRemoteResultMessage(transportMessage.GetContent<RemoteResultMessage>()));
            }
        }

        [ProtoMember(1)] public string Id { get; set; }

        [ProtoMember(2)] public byte[] Content { get; set; }

        [ProtoMember(3)] public string ContentType { get; set; }

        public TransportMessage GetTransportMessage()
        {
            var message = new TransportMessage
            {
                ContentType = ContentType,
                Id = Id
            };

            object contentObject;
            if (ContentType == TransportMessageType.RemoteInvokeMessage)
            {
                contentObject =
                    SerializerUtilitys.Deserialize<ProtoBufRemoteInvokeMessage>(Content).GetMessage();
            }
            else if (ContentType == TransportMessageType.RemoteResultMessage)
            {
                contentObject =
                    SerializerUtilitys.Deserialize<ProtoBufRemoteResultMessage>(Content).GetMessage();
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }

            message.Content = contentObject;
            return message;
        }
    }
}