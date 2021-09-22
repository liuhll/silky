using System;
using Nacos.V2;
using Nacos.V2.DependencyInjection;
using Silky.Core;
using Silky.RegistryCenter.Nacos;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NacosServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosRegistryCenter(this IServiceCollection services,
            string section = "RegistryCenter")
        {
            services.Configure<NacosRegistryCenterOptions>(EngineContext.Current.Configuration.GetSection(section));

            services.AddNacosV2Naming(EngineContext.Current.Configuration, sectionName: section);
            services.AddSingleton<IServerRegister,NacosServerRegister>();

            return services;
        }

        public static IServiceCollection AddNacosRegistryCenter(this IServiceCollection services,
            Action<NacosSdkOptions> optionsAccs)
        {
            services.Configure(optionsAccs);

            var options = new NacosRegistryCenterOptions();
            optionsAccs.Invoke(options);

            services.AddNacosV2Naming(options.BuildSdkOptions());

            return services;
        }
    }
}