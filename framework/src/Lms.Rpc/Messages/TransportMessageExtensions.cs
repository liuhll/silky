namespace Lms.Rpc.Messages
{
    public static class TransportMessageExtensions
    {
        public static bool IsInvokeMessage(this TransportMessage message)
        {
            return message.ContentType == TransportMessageType.RemoteInvokeMessage;
        }
        
        public static bool IsResultMessage(this TransportMessage message)
        {
            return message.ContentType == TransportMessageType.RemoteResultMessage;
        }
    }
}