using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nacos.V2;
using Nacos.V2.DependencyInjection;
using Silky.Core;
using Silky.RegistryCenter.Nacos;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = Silky.RegistryCenter.Nacos.IServiceProvider;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NacosServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosRegistryCenter(this IServiceCollection services,
            string section = "RegistryCenter")
        {
            services.Configure<NacosRegistryCenterOptions>(EngineContext.Current.Configuration.GetSection(section));

            services.AddNacosV2Naming(EngineContext.Current.Configuration, sectionName: section);
            services.AddNacosV2Config(EngineContext.Current.Configuration, sectionName: section);
            services.AddNacosRegistryCenterCore();
            return services;
        }

        public static IServiceCollection AddNacosRegistryCenter(this IServiceCollection services,
            Action<NacosRegistryCenterOptions> optionsAction)
        {
            services.Configure(optionsAction);

            var options = new NacosRegistryCenterOptions();
            optionsAction.Invoke(options);

            services.AddNacosV2Naming(options.BuildSdkOptions());
            services.AddNacosV2Config(options.BuildSdkOptions());
            services.AddNacosRegistryCenterCore();

            return services;
        }

        private static IServiceCollection AddNacosRegistryCenterCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IServerRegister, NacosServerRegister>();
            services.TryAddSingleton<IServiceProvider, NacosServiceProvider>();
            services.TryAddSingleton<IServerRegisterProvider, NacosServerRegisterProvider>();
            services.TryAddSingleton<IRegisterCenterHealthProvider, NocasRegisterCenterHealthProvider>();
            return services;
        }
    }
}