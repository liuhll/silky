using Nacos.V2;
using Silky.Rpc.Configuration;

namespace Silky.RegistryCenter.Nacos.Configuration
{
    public class NacosRegistryCenterOptions : NacosSdkOptions, IRegistryCenterOptions
    {
        internal static string RegistryCenterSection = "RegistryCenter";

        public string Type { get; } = "Nacos";
        
        public string ServerGroupName { get; set; } = "Server";
        
        public string ServerKey = "silky.servers";
        
        public string SwaggerDocKey = "silky.swaggers";

        public string SwaggerDocGroupName = "Swagger";
        
        public bool RegisterSwaggerDoc { get; set; } = true;
        

        public System.Action<NacosSdkOptions> BuildSdkOptions()
        {
            return x =>
            {
                x.AccessKey = this.AccessKey;
                x.ConfigUseRpc = this.ConfigUseRpc;
                x.ContextPath = this.ContextPath;
                x.DefaultTimeOut = this.DefaultTimeOut;
                x.EndPoint = this.EndPoint;
                x.ListenInterval = this.ListenInterval;
                x.Namespace = this.Namespace;
                x.NamingLoadCacheAtStart = this.NamingLoadCacheAtStart;
                x.NamingUseRpc = this.NamingUseRpc;
                x.Password = this.Password;
                x.SecretKey = this.SecretKey;
                x.ServerAddresses = this.ServerAddresses;
                x.UserName = this.UserName;
            };
        }
    }
}