namespace Lms.Rpc.Configuration
{
    public class RpcOptions 
    {
        public static string Rpc = "Rpc";

        public string Host { get; set; } = "0.0.0.0";
        public int RpcPort { get; set; } = 100;
        public int MqttPort { get; set; } = 200;
        public bool UseLibuv { get; set; } = true;
        public bool IsSsl { get; set; } = false;

        public string SslCertificateName { get; set; }

        public string SslCertificatePassword { get; set; }
        public int SoBacklog { get; set; } = 8192;
    }
}