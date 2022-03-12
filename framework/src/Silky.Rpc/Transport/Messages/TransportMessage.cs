using System;
using JetBrains.Annotations;

namespace Silky.Rpc.Transport.Messages
{
    [Serializable]
    public class TransportMessage
    {
        public TransportMessage()
        {
        }

        public TransportMessage([NotNull] object content, [NotNull] string id)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ContentType = content.GetType().FullName;
            if (ContentType != TransportMessageType.RemoteInvokeMessage &&
                ContentType != TransportMessageType.RemoteResultMessage)
            {
                throw new ArgumentException(nameof(content));
            }
        }

        public string Id { get; set; }

        public string ContentType { get; set; }

        public object Content { get; set; }

        public T GetContent<T>() where T : IRemoteMessage
        {
            return (T)Content;
        }
    }
}