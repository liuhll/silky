using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.RegistryCenter.Consul.HealthCheck;
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
            services.AddConsulRegistryCenter();
            return services;
        }

        public static IServiceCollection AddConsulRegistryCenter(
            this IServiceCollection services,
            Action<ConsulRegistryCenterOptions> configure)
        {
            services.Configure(configure);
            var options = new ConsulRegistryCenterOptions();
            configure.Invoke(options);
            services.AddConsulRegistryCenter();
            return services;
        }

        private static IServiceCollection AddConsulRegistryCenter(this IServiceCollection services)
        {
            services.TryAddSingleton<IServerRegister, ConsulServerRegister>();
            services.TryAddSingleton<IConsulClientFactory, ConsulClientFactory>();
            services.TryAddSingleton<IServiceDescriptorProvider, ConsulServiceDescriptorProvider>();
            services.TryAddSingleton<IServerConverter, ConsulServerConverter>();
            services.TryAddSingleton<IHealthCheckService, ConsulHealthCheckService>();
            services.TryAddSingleton<IRegisterCenterHealthProvider, ConsulRegisterCenterHealthProvider>();
            return services;
        }
    }
}