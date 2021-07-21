using System.Diagnostics.CodeAnalysis;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode([NotNull]byte[] data);
    }
}