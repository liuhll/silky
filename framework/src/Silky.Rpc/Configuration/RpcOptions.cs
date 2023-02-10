using JetBrains.Annotations;

namespace Silky.Rpc.Configuration
{
    public class RpcOptions
    {
        internal static string Rpc = "Rpc";

        public RpcOptions()
        {
            Host = "0.0.0.0";
            Port = 2200;
            UseLibuv = true;
            IsSsl = false;
            SoBacklog = 1024;
            ConnectTimeout = 300;
            RegisterFailureRetryCount = 10;
            TransportClientPoolNumber = 10;
            UseTransportClientPool = false;
            EnableMonitor = false;
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseLibuv { get; set; }
        public bool IsSsl { get; set; }
        public string SslCertificateName { get; set; }
        public string SslCertificatePassword { get; set; }
        public int SoBacklog { get; set; }
        [NotNull] public string Token { get; set; }
        public double ConnectTimeout { get; set; }
        public int RegisterFailureRetryCount { get; set; }
        public int TransportClientPoolNumber { get; set; }
        public bool UseTransportClientPool { get; set; }
        public bool EnableMonitor { get; set; }
    }
}