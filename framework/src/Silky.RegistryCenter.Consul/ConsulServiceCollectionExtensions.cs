using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public static class ConsulServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulRegistryCenter(this IServiceCollection services,
            string section = "RegistryCenter")
        {
            services.Configure<ConsulRegistryCenterOptions>(EngineContext.Current.Configuration.GetSection(section));
            services.AddSingleton<IServerRegister, ConsulServerRegister>();
            services.TryAddSingleton<IConsulClientFactory, ConsulClientFactory>();
            services.TryAddSingleton<IServiceProvider, ConsulServiceProvider>();
            services.TryAddSingleton<IServerConverter, ConsulServerConverter>();

            return services;
        }

        public static IServiceCollection AddConsulRegistryCenter(
            this IServiceCollection services,
            Action<ConsulRegistryCenterOptions> configure)
        {
            services.Configure(configure);
            var options = new ConsulRegistryCenterOptions();
            configure.Invoke(options);
            services.AddSingleton<IServerRegister, ConsulServerRegister>();
            services.TryAddSingleton<IConsulClientFactory, ConsulClientFactory>();
            services.TryAddSingleton<IServiceProvider, ConsulServiceProvider>();
            services.TryAddSingleton<IServerConverter, ConsulServerConverter>();
            return services;
        }
    }
}