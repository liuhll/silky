namespace Lms.Rpc.Configuration
{
    public class RpcOptions 
    {
        public static string Rpc = "Rpc";

        public string Host { get; set; } = "0.0.0.0";
        public int RpcPort { get; set; } = 100;
        public int MqttPort { get; set; } = 200;
    }
}