namespace Silky.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        public static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; } = RegistryCenterType.Zookeeper;

        public double ConnectionTimeout { get; set; } = 5000;

        public double SessionTimeout { get; set; } = 8000;

        public double OperatingTimeout { get; set; } = 10000;

        public string ConnectionStrings { get; set; }

        public int FuseTimes { get; set; } = 3;

        public string RoutePath { get; set; } = "/services/serviceroutes";

        public string GatewayPath { get; set; } = "/gateways";
    }
}