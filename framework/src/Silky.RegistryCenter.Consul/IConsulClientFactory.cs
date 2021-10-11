using Consul;

namespace Silky.RegistryCenter.Consul
{
    public interface IConsulClientFactory
    {
        IConsulClient CreateClient();
    }
}