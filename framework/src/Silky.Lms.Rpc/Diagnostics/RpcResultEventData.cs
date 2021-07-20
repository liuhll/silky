namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcResultEventData
    {
        public string MessageId { get; set; }
        public string ServiceId { get; set; }

        public object Result { get; set; }

        public string RemoteAddress { get; set; }

        public long? ElapsedTimeMs { get; set; }
    }
}