using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public static class ConsulServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulRegistryCenter(this IServiceCollection services,
            string section = "RegistryCenter")
        {
            services.Configure<ConsulRegistryCenterOptions>(EngineContext.Current.Configuration.GetSection(section));
            services.TryAddSingleton<IServerRegister, ConsulServerRegister>();
            services.TryAddSingleton<IConsulClientFactory, ConsulClientFactory>();
            services.TryAddSingleton<IServiceProvider, ConsulServiceProvider>();
            services.TryAddSingleton<IServerConverter, ConsulServerConverter>();
            services.AddSingleton<IRegisterCenterHealthProvider, ConsulRegisterCenterHealthProvider>();
            return services;
        }

        public static IServiceCollection AddConsulRegistryCenter(
            this IServiceCollection services,
            Action<ConsulRegistryCenterOptions> configure)
        {
            services.Configure(configure);
            var options = new ConsulRegistryCenterOptions();
            configure.Invoke(options);
            services.TryAddSingleton<IServerRegister, ConsulServerRegister>();
            services.TryAddSingleton<IConsulClientFactory, ConsulClientFactory>();
            services.TryAddSingleton<IServiceProvider, ConsulServiceProvider>();
            services.TryAddSingleton<IServerConverter, ConsulServerConverter>();
            services.TryAddSingleton<IHeartBeatService, DefaultHeartBeatService>();
            services.AddSingleton<IRegisterCenterHealthProvider, ConsulRegisterCenterHealthProvider>();
            return services;
        }
    }
}