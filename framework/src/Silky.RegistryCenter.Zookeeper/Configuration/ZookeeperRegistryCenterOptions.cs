using Silky.Rpc.Configuration;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Configuration
{
    public class ZookeeperRegistryCenterOptions : IRegistryCenterOptions
    {
        internal static string RegistryCenter = "RegistryCenter";

        public string Type { get; } = "Zookeeper";

        public int HeartBeatIntervalSecond { get; set; } = 10;

        public bool EnableHeartBeat { get; set; } = true;

        public double ConnectionTimeout { get; set; } = 3000;

        public double SessionTimeout { get; set; } = 20000;

        public double OperatingTimeout { get; set; } = 30000;

        public string ConnectionStrings { get; set; }

        public int FuseTimes { get; set; } = 10;

        public AuthScheme Scheme { get; set; } = AuthScheme.World;

        public string Auth { get; set; } = "anyone";

        public bool RegisterSwaggerDoc { get; set; } = true;

        public string RoutePath { get; set; } = "/silky/server";

        public string SwaggerDocPath { get; set; } = "/silky/swagger";
    }
}