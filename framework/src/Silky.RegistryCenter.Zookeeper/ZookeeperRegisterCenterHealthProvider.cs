using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.RegistryCenters;

namespace Silky.RegistryCenter.Zookeeper
{
    public class ZookeeperRegisterCenterHealthProvider : IRegisterCenterHealthProvider
    {
        private readonly IZookeeperClientFactory _zookeeperClientFactory;

        public ZookeeperRegisterCenterHealthProvider(IZookeeperClientFactory zookeeperClientFactory)
        {
            _zookeeperClientFactory = zookeeperClientFactory;
        }

        public IDictionary<string, RegistryCenterHealthCheckModel> GetRegistryCenterHealthInfo()
        {
            var registryCenterHealthDict = new Dictionary<string, RegistryCenterHealthCheckModel>();
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                registryCenterHealthDict.Add(zookeeperClient.Options.ConnectionString,
                    _zookeeperClientFactory.GetHealthCheckInfo(zookeeperClient));
            }

            return registryCenterHealthDict;
        }
    }
}