namespace Lms.WebSocket.Configuration
{
    public class WebSocketOptions
    {
        public static string WebSocket = "WebSocket";

        public int WsPort { get; set; } = 3000;

        public bool IsSsl { get; set; } = false;
        public string SslCertificateName { get; set; }
        public string SslCertificatePassword { get; set; }

        public bool IgnoreExtensions { get; set; }

        public bool EmitOnPing { get; set; }

        public string Protocol { get; set; }

        public bool AllowForwardedRequest { get; set; } = true;

        public int WaitTime { get; set; } = 1;

        public bool KeepClean { get; set; } = false;
    }
}