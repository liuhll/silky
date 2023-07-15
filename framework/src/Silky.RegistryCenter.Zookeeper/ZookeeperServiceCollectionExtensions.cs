using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core;
using Silky.RegistryCenter.Zookeeper;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ZookeeperServiceCollectionExtensions
    {
        public static IServiceCollection AddZookeeperRegistryCenter(this IServiceCollection services,
            string section = "RegistryCenter")
        {
            services.AddOptions<ZookeeperRegistryCenterOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(section));
            services.TryAddSingleton<IServerRegister, ZookeeperServerRegister>();
            services.TryAddSingleton<IZookeeperClientFactory, DefaultZookeeperClientFactory>();
            services.TryAddSingleton<IZookeeperStatusChange, ZookeeperServerRegister>();
            services.TryAddSingleton<IRegisterCenterHealthProvider, ZookeeperRegisterCenterHealthProvider>();
            return services;
        }
    }
}