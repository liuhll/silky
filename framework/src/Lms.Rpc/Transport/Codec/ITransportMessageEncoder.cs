using System.Diagnostics.CodeAnalysis;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode([NotNull]TransportMessage message);
    }
}