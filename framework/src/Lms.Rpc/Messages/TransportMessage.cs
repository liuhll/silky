using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Lms.Core.Exceptions;
using Lms.Core.Utils;

namespace Lms.Rpc.Messages
{
    [Serializable]
    public class TransportMessage
    {
        public TransportMessage()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage([NotNull] object content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            Id = GuidGenerator.CreateGuidStrWithNoUnderline();
            Content = content;
            ContentType = content.GetType().FullName;
            if (ContentType != TransportMessageType.RemoteInvokeMessage &&
                ContentType != TransportMessageType.RemoteResultMessage)
            {
                throw new ArgumentException(nameof(content));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage([NotNull] object content, [NotNull]string id)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetContent<T>() where T : IRemoteMessage
        {
            if (typeof(T).FullName != ContentType)
            {
                throw new LmsException("指定的消息数据类型不正确");
            }

            return (T) Content;
        }
    }
}