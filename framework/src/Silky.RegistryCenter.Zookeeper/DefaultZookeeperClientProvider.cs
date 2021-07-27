using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Zookeeper;
using Silky.Zookeeper.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public class DefaultZookeeperClientProvider : IDisposable, IZookeeperClientProvider
    {
        private ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients = new ();
        
        
        private readonly RegistryCenterOptions _registryCenterOptions;
        public ILogger<DefaultZookeeperClientProvider> Logger { get; set; }

        public DefaultZookeeperClientProvider(IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _registryCenterOptions = registryCenterOptions.Value;
            Check.NotNullOrEmpty(_registryCenterOptions.ConnectionStrings,
                nameof(_registryCenterOptions.ConnectionStrings));
            Logger = NullLogger<DefaultZookeeperClientProvider>.Instance;
            CreateZookeeperClients();
        }

        private void CreateZookeeperClients()
        {
            var connStrs = _registryCenterOptions.ConnectionStrings.Split(";");
            foreach (var connStr in connStrs)
            {
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
                    if (connectionStateChangeArgs.State == Watcher.Event.KeeperState.Expired)
                    {
                        if (client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected,
                            zookeeperClientOptions.ConnectionTimeout))
                        {
                            _zookeeperClients.Remove(client.Options.ConnectionString, out _);
                        }
                    }
                });
                _zookeeperClients.GetOrAdd(connStr, zookeeperClient);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"无法链接服务注册中心{connStr},原因:{e.Message}");
            }
        }

        public IZookeeperClient GetZooKeeperClient()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new SilkyException("当前没有可用服务注册中心");
            }

            if (_zookeeperClients.Count == 1)
            {
                return _zookeeperClients.First().Value;
            }

            return _zookeeperClients.Values.ToArray()[RondomSelectorIndex(0, _zookeeperClients.Count)];
        }

        private int RondomSelectorIndex(int min, int max)
        {
            var random = new Random((int) DateTime.Now.Ticks);
            return random.Next(min, max);
        }

        public IReadOnlyList<IZookeeperClient> GetZooKeeperClients()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new SilkyException("当前没有可用服务注册中心");
            }

            return _zookeeperClients.Values.ToImmutableList();
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