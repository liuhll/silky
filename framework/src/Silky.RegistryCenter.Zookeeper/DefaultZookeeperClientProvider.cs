using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Castle.Core.Internal;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Zookeeper;
using Silky.Zookeeper.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Routing;
using Silky.Rpc.Utils;

namespace Silky.RegistryCenter.Zookeeper
{
    public class DefaultZookeeperClientProvider : IDisposable, IZookeeperClientProvider
    {
        private ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients = new();

        private ConcurrentDictionary<string, RegistryCenterHealthCheckModel> m_healthCheck = new();

        private RegistryCenterOptions _registryCenterOptions;
        public ILogger<DefaultZookeeperClientProvider> Logger { get; set; }

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

        public DefaultZookeeperClientProvider(IOptionsMonitor<RegistryCenterOptions> registryCenterOptions)
        {
            _registryCenterOptions = registryCenterOptions.CurrentValue;

            Check.NotNullOrEmpty(_registryCenterOptions.ConnectionStrings,
                nameof(_registryCenterOptions.ConnectionStrings));
            Logger = NullLogger<DefaultZookeeperClientProvider>.Instance;

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
                    if (connectionStateChangeArgs.State == Watcher.Event.KeeperState.Expired)
                    {
                        if (!client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected,
                            zookeeperClientOptions.ConnectionTimeout))
                        {
                            healthCheckModel.IsHealth = false;
                            healthCheckModel.UnHealthTimes += 1;
                            healthCheckModel.UnHealthType = UnHealthType.ConnectionTimeout;
                            healthCheckModel.UnHealthReason = "Connection session expired";
                            if (healthCheckModel.UnHealthTimes > _registryCenterOptions.FuseTimes)
                            {
                                _zookeeperClients.Remove(client.Options.ConnectionString, out _);
                            }
                        }
                        else
                        {
                            if (healthCheckModel.UnHealthType == UnHealthType.Disconnected)
                            {
                                var serviceRouteManager = EngineContext.Current.Resolve<IZookeeperStatusChange>();
                                await serviceRouteManager.CreateSubscribeServiceRouteDataChanges(client);
                            }

                            healthCheckModel.SetHealth();
                        }

                        m_healthCheck.AddOrUpdate(connStr, healthCheckModel, (k, v) => healthCheckModel);
                    }

                    if (connectionStateChangeArgs.State == Watcher.Event.KeeperState.Disconnected)
                    {
                        healthCheckModel.IsHealth = false;
                        healthCheckModel.UnHealthTimes += 1;
                        healthCheckModel.UnHealthReason = "Connection session disconnected";
                        healthCheckModel.UnHealthType = UnHealthType.Disconnected;
                        m_healthCheck.AddOrUpdate(connStr, healthCheckModel, (k, v) => healthCheckModel);
                    }
                });
                _zookeeperClients.GetOrAdd(connStr, zookeeperClient);
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(true, 0));
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Unable to link to the service registry {connStr}, reason: {e.Message}");
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(false)
                {
                    UnHealthReason = e.Message
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