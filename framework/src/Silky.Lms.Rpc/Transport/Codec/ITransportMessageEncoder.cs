using System.Diagnostics.CodeAnalysis;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode([NotNull]TransportMessage message);
    }
}