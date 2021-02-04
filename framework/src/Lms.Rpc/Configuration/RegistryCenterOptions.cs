namespace Lms.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        public static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; }

        public double ConnectionTimeout { get; set; } = 20;
        
        public double SessionTimeout { get; set; } = 30;

        public double OperatingTimeout { get; set; } = 60;

        public string ConnectionStrings { get; set; }

        public string RoutePath { get; set; } = "/services/serviceroutes";

        public string MqttPtah { get; set; }= "/services/servicemqttroutes";
        public string CommandPath { get; set; } = "/services/servicecommands";
        public double HealthCheckInterval { get; set; } = 30;
    }
}