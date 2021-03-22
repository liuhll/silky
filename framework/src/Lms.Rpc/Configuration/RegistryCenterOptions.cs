namespace Lms.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        public static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; } = RegistryCenterType.Zookeeper;

        public double ConnectionTimeout { get; set; } = 1000;

        public double SessionTimeout { get; set; } = 2000;

        public double OperatingTimeout { get; set; } = 4000;

        public string ConnectionStrings { get; set; }

        public string RoutePath { get; set; } = "/services/serviceroutes";
        public double HealthCheckInterval { get; set; } = 30;
    }
}