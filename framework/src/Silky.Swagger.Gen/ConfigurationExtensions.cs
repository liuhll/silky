using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.RegistryCenter.Zookeeper.Configuration;

namespace Silky.Rpc.Configuration;

public static class ConfigurationExtensions
{
    public static IRegistryCenterOptions GetRegistryCenterOptions(this IConfiguration configuration)
    {
        var registerType = configuration.GetValue<string>("registrycenter:type");
        if (registerType.IsNullOrEmpty())
        {
            throw new SilkyException("You did not specify the service registry type");
        }

        IRegistryCenterOptions registryCenterOption = null;
        switch (registerType.ToLower())
        {
            case "zookeeper":
                registryCenterOption = EngineContext.Current.GetOptions<ZookeeperRegistryCenterOptions>();
                break;
            case "nacos":
                registryCenterOption = EngineContext.Current.GetOptions<NacosRegistryCenterOptions>();
                break;
            case "consul":
                registryCenterOption = EngineContext.Current.GetOptions<ConsulRegistryCenterOptions>();
                break;
            default:
                throw new SilkyException(
                    $"The system does not provide a service registration center of type {registerType}");
        }
        return registryCenterOption;
    }
}