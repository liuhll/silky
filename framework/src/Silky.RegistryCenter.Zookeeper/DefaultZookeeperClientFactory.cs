using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Castle.Core.Internal;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Zookeeper;
using Silky.Zookeeper.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using Silky.Core.Logging;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.Rpc.RegistryCenters;

namespace Silky.RegistryCenter.Zookeeper
{
    public class DefaultZookeeperClientFactory : IDisposable, IZookeeperClientFactory
    {
        private ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients = new();

        private ConcurrentDictionary<string, RegistryCenterHealthCheckModel> m_healthCheck = new();

        private ZookeeperRegistryCenterOptions _registryCenterOptions;
        public ILogger<DefaultZookeeperClientFactory> Logger { get; set; }

        protected string[] ConnectionStrings
        {
            get
            {
                var connectionStrings =
                    _registryCenterOptions?.ConnectionStrings?.Split(";")?.Where(p => !p.IsNullOrEmpty()).ToArray() ??
                    new string[0];
                return connectionStrings;
            }
        }

        public DefaultZookeeperClientFactory(IOptions<ZookeeperRegistryCenterOptions> registryCenterOptions)
        {
            _registryCenterOptions = registryCenterOptions.Value;

            Check.NotNullOrEmpty(_registryCenterOptions.ConnectionStrings,
                nameof(_registryCenterOptions.ConnectionStrings));
            Logger = NullLogger<DefaultZookeeperClientFactory>.Instance;

            CreateZookeeperClients();
        }

        private void CreateZookeeperClients()
        {
            foreach (var connStr in ConnectionStrings)
            {
                if (connStr.IsNullOrEmpty()) continue;

                if (!_zookeeperClients.Keys.Contains(connStr))
                {
                    CreateZookeeperClient(connStr);
                }
            }
        }

        private void CreateZookeeperClient(string connStr)
        {
            var zookeeperClientOptions = new ZookeeperClientOptions(connStr)
            {
                ConnectionTimeout = TimeSpan.FromMilliseconds(_registryCenterOptions.ConnectionTimeout),
                OperatingTimeout = TimeSpan.FromMilliseconds(_registryCenterOptions.OperatingTimeout),
                SessionTimeout = TimeSpan.FromMilliseconds(_registryCenterOptions.SessionTimeout),
            };
            try
            {
                var zookeeperClient = new ZookeeperClient(zookeeperClientOptions);
                zookeeperClient.SubscribeStatusChange(async (client, connectionStateChangeArgs) =>
                {
                    var healthCheckModel = m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(true, 0));

                    switch (connectionStateChangeArgs.State)
                    {
                        case Watcher.Event.KeeperState.Disconnected:
                        case Watcher.Event.KeeperState.Expired:
                            if (client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected,
                                zookeeperClientOptions.ConnectionTimeout))
                            {
                                if (healthCheckModel.HealthType == HealthType.Disconnected)
                                {
                                    var zookeeperStatusChange = EngineContext.Current.Resolve<IZookeeperStatusChange>();
                                    await zookeeperStatusChange.CreateSubscribeServersChange(client);
                                }

                                healthCheckModel.SetHealth();
                            }
                            else
                            {
                                healthCheckModel.SetUnHealth(HealthType.Disconnected,
                                    "Connection session disconnected");
                                if (healthCheckModel.UnHealthTimes > _registryCenterOptions.FuseTimes)
                                {
                                    _zookeeperClients.Remove(client.Options.ConnectionString, out _);
                                }
                            }

                            break;
                        case Watcher.Event.KeeperState.AuthFailed:
                            healthCheckModel.SetUnHealth(HealthType.AuthFailed, "AuthFailed");
                            break;
                        case Watcher.Event.KeeperState.SyncConnected:
                        case Watcher.Event.KeeperState.ConnectedReadOnly:
                            healthCheckModel.SetHealth();
                            break;
                    }
                });
                _zookeeperClients.GetOrAdd(connStr, zookeeperClient);
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(true, 0));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(false)
                {
                    HealthType = HealthType.Disconnected,
                    UnHealthTimes = 1,
                    UnHealthReason = ex.Message
                });
            }
        }

        public IZookeeperClient GetZooKeeperClient()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new SilkyException("There is currently no service registry available");
            }

            if (_zookeeperClients.Count == 1)
            {
                return _zookeeperClients.First().Value;
            }

            return _zookeeperClients.Values.ToArray()[RandomSelectorIndex(0, _zookeeperClients.Count)];
        }

        private int RandomSelectorIndex(int min, int max)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return random.Next(min, max);
        }

        public IReadOnlyList<IZookeeperClient> GetZooKeeperClients()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new SilkyException("There is currently no service registry available");
            }

            return _zookeeperClients.Values.ToImmutableList();
        }

        public RegistryCenterHealthCheckModel GetHealthCheckInfo(IZookeeperClient zookeeperClient)
        {
            return m_healthCheck.GetValueOrDefault(zookeeperClient.Options.ConnectionString);
        }

        public void Dispose()
        {
            foreach (var _client in _zookeeperClients)
            {
                _client.Value?.Dispose();
            }
        }
    }
}