using System;
using Lms.Rpc.Messages;
using MessagePack;

namespace Lms.Codec.Message
{
    [MessagePackObject]
    public class MessagePackTransportMessage
    {
        public MessagePackTransportMessage()
        {
        }

        public MessagePackTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;
            if (transportMessage.IsInvokeMessage())
            {
                Content = SerializerUtilitys.Serialize(
                    new MessagePackRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>()));
            }
            
            if (transportMessage.IsResultMessage())
            {
                Content = SerializerUtilitys.Serialize(
                    new MessagePackRemoteResultMessage(transportMessage.GetContent<RemoteResultMessage>()));
            }
        }

        [Key(0)] public string Id { get; set; }

        [Key(1)] public byte[] Content { get; set; }

        [Key(2)] public string ContentType { get; set; }

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
                    SerializerUtilitys.Deserialize<MessagePackRemoteInvokeMessage>(Content).GetMessage();
            }
            else if (ContentType == TransportMessageType.RemoteResultMessage)
            {
                contentObject =
                    SerializerUtilitys.Deserialize<MessagePackRemoteResultMessage>(Content).GetMessage();
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