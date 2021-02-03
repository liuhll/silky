using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Configuration;
using Lms.Zookeeper;
using Microsoft.Extensions.Options;

namespace Lms.RegistryCenter.Zookeeper
{
    internal class DefaultZookeeperClientProvider : IZookeeperClientProvider
    {
        private readonly RegistryCenterOptions _registryCenterOptions;

        public DefaultZookeeperClientProvider(IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _registryCenterOptions = registryCenterOptions.Value;
        }

        public Task<IZookeeperClient> GetZooKeeperClient()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<IZookeeperClient>> GetZooKeeperClients()
        {
            throw new System.NotImplementedException();
        }
    }
}