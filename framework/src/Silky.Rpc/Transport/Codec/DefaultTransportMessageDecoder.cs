using JetBrains.Annotations;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport.Codec
{
    public sealed class DefaultTransportMessageDecoder : ITransportMessageDecoder
    {
        private ISerializer _serializer;

        public DefaultTransportMessageDecoder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public TransportMessage Decode([NotNull] byte[] data)
        {
            var jsonString = data.GetString();
            var transportMessage = _serializer.Deserialize<TransportMessage>(jsonString);
            if (transportMessage.IsInvokeMessage())
            {
                transportMessage.Content =
                    _serializer.Deserialize<RemoteInvokeMessage>(transportMessage.Content.ToString());
            }

            if (transportMessage.IsResultMessage())
            {
                transportMessage.Content =
                    _serializer.Deserialize<RemoteResultMessage>(transportMessage.Content.ToString());
            }

            return transportMessage;
        }
    }
}