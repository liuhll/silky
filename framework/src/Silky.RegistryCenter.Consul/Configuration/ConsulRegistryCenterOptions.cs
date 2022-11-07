using Consul;
using Silky.Rpc.Configuration;

namespace Silky.RegistryCenter.Consul.Configuration
{
    public class ConsulRegistryCenterOptions : ConsulClientConfiguration, IRegistryCenterOptions
    {
        internal static string RegistryCenterSection = "RegistryCenter";

        internal const string SilkyServer = "SilkyServer";

        public string Type { get; } = "Consul";

        public bool RegisterSwaggerDoc { get; set; } = true;

        public int HeartBeatIntervalSecond { get; set; } = 10;
        
        public string RoutePath { get; set; } = "silky/services";

        public string SwaggerDocPath { get; set; } = "silky/swagger";
    }
}