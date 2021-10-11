using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport.Codec
{
    public sealed class DefaultTransportMessageEncoder : ITransportMessageEncoder
    {
        private ISerializer _serializer;

        public DefaultTransportMessageEncoder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] Encode([NotNull] TransportMessage message)
        {
            Check.NotNull(message, nameof(message));
            var jsonString = _serializer.Serialize(message);
            return jsonString.GetBytes();
        }
    }
}