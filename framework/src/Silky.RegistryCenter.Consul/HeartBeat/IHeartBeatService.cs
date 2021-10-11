using System;
using System.Threading.Tasks;

namespace Silky.RegistryCenter.Consul
{
    public interface IHeartBeatService : IDisposable
    {
        void Start(Func<Task> cacheServerFromConsul);
    }
}