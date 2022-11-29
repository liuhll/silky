using System.Threading.Tasks;
using Consul;

namespace Silky.RegistryCenter.Consul.HealthCheck;

public interface IHealthCheckService
{
    Task<string[]> Check(IConsulClient consulClient, string service);
}