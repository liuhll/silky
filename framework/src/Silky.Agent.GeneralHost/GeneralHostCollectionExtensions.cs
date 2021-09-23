using Silky.Core.Exceptions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GeneralHostCollectionExtensions
    {
        public static IServiceCollection AddDefaultRegistryCenter(this IServiceCollection services, string registerType)
        {
            switch (registerType.ToLower())
            {
                case "zookeeper":
                    services.AddZookeeperRegistryCenter();
                    break;
                case "nacos":
                    services.AddNacosRegistryCenter();
                    break;
                default:
                    throw new SilkyException($"The system does not provide a service registration center of type {registerType}");
            }
            
            return services;
        }
    }
}