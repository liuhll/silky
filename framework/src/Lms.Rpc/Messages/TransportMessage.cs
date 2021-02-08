using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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
        public TransportMessage([NotNull]object content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            Id = GuidGenerator.CreateGuidStrWithNoUnderline();
            Content = content;
            ContentType = content.GetType().FullName;
            if (ContentType != TransportMessageType.RemoteInvokeMessage || ContentType != TransportMessageType.RemoteResultMessage)
            {
                throw new ArgumentException(nameof(content));
            }
        }
        
        public string Id { get; set; }
        
        public string ContentType { get; set; }
        
        public object Content { get; set; }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetContent<T>()
        {
            return (T)Content;
        }
        
    }
}