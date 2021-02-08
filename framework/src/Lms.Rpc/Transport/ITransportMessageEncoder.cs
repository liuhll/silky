using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}