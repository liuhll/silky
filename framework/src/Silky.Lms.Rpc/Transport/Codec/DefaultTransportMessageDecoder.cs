using JetBrains.Annotations;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport.Codec
{
    public sealed class DefaultTransportMessageDecoder : ITransportMessageDecoder
    {
        private ISerializer _serializer;

        public DefaultTransportMessageDecoder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public TransportMessage Decode([NotNull]byte[] data)
        {
            var jsonString = data.GetString();
            var transportMessage = _serializer.Deserialize<TransportMessage>(jsonString);
            if (transportMessage.IsInvokeMessage())
            {
                transportMessage.Content = _serializer.Deserialize<RemoteInvokeMessage>(transportMessage.Content.ToString());
            }
            if (transportMessage.IsResultMessage())
            {
                transportMessage.Content = _serializer.Deserialize<RemoteResultMessage>(transportMessage.Content.ToString());
            }

            return transportMessage;
        }
    }
}