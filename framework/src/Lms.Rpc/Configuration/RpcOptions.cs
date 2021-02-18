using JetBrains.Annotations;

namespace Lms.Rpc.Configuration
{
    public class RpcOptions
    {
        public static string Rpc = "Rpc";

        public string Host { get; set; } = "0.0.0.0";
        public int RpcPort { get; set; } = 2200;
        public int MqttPort { get; set; } = 2300;
        public bool UseLibuv { get; set; } = true;
        public bool IsSsl { get; set; } = false;
        public string SslCertificateName { get; set; }
        public string SslCertificatePassword { get; set; }
        public int SoBacklog { get; set; } = 8192;

        public bool RemoveUnhealthServer { get; set; } = true;

        [NotNull] public string Token { get; set; }
        public double ConnectTimeout { get; set; } = 500;
    }
}