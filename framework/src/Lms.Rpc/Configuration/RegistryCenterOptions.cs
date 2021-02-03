namespace Lms.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        public static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; }

        public double SessionTimeout { get; set; } = 20;

        public double ConnectionTimeout { get; set; } = 10;

        public double OperatingTimeout { get; set; } = 20;

        public string ConnectionString { get; set; }

        public string RoutePath { get; set; } = "/services/serviceRoutes";

        public string CommandPath { get; set; } = "/services/serviceCommands";
    }
}