namespace Silky.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        public static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; } = RegistryCenterType.Zookeeper;

        public double ConnectionTimeout { get; set; } = 10000;

        public double SessionTimeout { get; set; } = 20000;

        public double OperatingTimeout { get; set; } = 40000;

        public string ConnectionStrings { get; set; }

        public string RoutePath { get; set; } = "/services/serviceroutes";
    }
}