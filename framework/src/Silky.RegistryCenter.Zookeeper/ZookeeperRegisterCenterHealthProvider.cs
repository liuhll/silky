using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.RegistryCenters;

namespace Silky.RegistryCenter.Zookeeper
{
    public class ZookeeperRegisterCenterHealthProvider : IRegisterCenterHealthProvider, ITransientDependency
    {
        private readonly IZookeeperClientProvider _zookeeperClientProvider;

        public ZookeeperRegisterCenterHealthProvider(IZookeeperClientProvider zookeeperClientProvider)
        {
            _zookeeperClientProvider = zookeeperClientProvider;
        }

        public IDictionary<string, RegistryCenterHealthCheckModel> GetRegistryCenterHealthInfo()
        {
            var registryCenterHealthDict = new Dictionary<string, RegistryCenterHealthCheckModel>();
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                registryCenterHealthDict.Add(zookeeperClient.Options.ConnectionString,
                    _zookeeperClientProvider.GetHealthCheckInfo(zookeeperClient));
            }

            return registryCenterHealthDict;
        }
    }
}