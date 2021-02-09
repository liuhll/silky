using JetBrains.Annotations;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport.Codec
{
    public sealed class DefaultTransportMessageEncoder : ITransportMessageEncoder
    {
        private ISerializer _serializer;

        public DefaultTransportMessageEncoder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] Encode([NotNull]TransportMessage message)
        {
            Check.NotNull(message, nameof(message));
            var jsonString = _serializer.Serialize(message);
            return jsonString.GetBytes();
        }
    }
}