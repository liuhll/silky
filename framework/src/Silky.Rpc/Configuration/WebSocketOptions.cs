namespace Silky.Rpc.Configuration
{
    public class WebSocketOptions
    {
        internal static string WebSocket = "WebSocket";

        public int Port { get; set; } = 3000;

        public bool IsSsl { get; set; } = false;
        public string SslCertificateName { get; set; }
        public string SslCertificatePassword { get; set; }

        public int WaitTime { get; set; } = 1;

        public bool KeepClean { get; set; } = false;

        public string Token { get; set; } = "websocket";
    }
}