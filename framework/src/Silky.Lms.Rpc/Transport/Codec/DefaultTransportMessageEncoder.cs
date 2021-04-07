using JetBrains.Annotations;
using Silky.Lms.Core;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport.Codec
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