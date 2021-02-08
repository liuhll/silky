using System.Diagnostics.CodeAnalysis;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode([NotNull]byte[] data);
    }
}