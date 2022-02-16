using System;
using Consul;
using Silky.Rpc.Configuration;

namespace Silky.RegistryCenter.Consul.Configuration
{
    public class ConsulRegistryCenterOptions : ConsulClientConfiguration, IRegistryCenterOptions
    {
        internal static string RegistryCenterSection = "RegistryCenter";

        internal const string SilkyServer = "SilkyServer";
        
        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 8500;

        public Uri Address
        {
            get => new Uri($"{Host}:{Port}");
        }
        
        public string Type { get; } = "Consul";
        public int HeartBeatInterval { get; set; } = 10;
    }
}
