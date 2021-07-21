using System.Diagnostics.CodeAnalysis;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode([NotNull]TransportMessage message);
    }
}