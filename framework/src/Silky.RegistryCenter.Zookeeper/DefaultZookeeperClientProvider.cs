using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Timers;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Zookeeper;
using Silky.Zookeeper.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Silky.RegistryCenter.Zookeeper
{
    public class DefaultZookeeperClientProvider : IDisposable, IZookeeperClientProvider
    {
        private ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients =
            new ConcurrentDictionary<string, IZookeeperClient>();

        private Timer _timer;
        private readonly RegistryCenterOptions _registryCenterOptions;
        public ILogger<DefaultZookeeperClientProvider> Logger { get; set; }

        public DefaultZookeeperClientProvider(IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _registryCenterOptions = registryCenterOptions.Value;
            Check.NotNullOrEmpty(_registryCenterOptions.ConnectionStrings,
                nameof(_registryCenterOptions.ConnectionStrings));
            Logger = NullLogger<DefaultZookeeperClientProvider>.Instance;
            CreateZookeeperClients();
            _timer = new Timer(TimeSpan.FromSeconds(_registryCenterOptions.HealthCheckInterval).TotalMilliseconds);
            _timer.Enabled = true;
            _timer.AutoReset = true;
            _timer.Elapsed += (sender, args) =>
            {
                foreach (var client in _zookeeperClients)
                {
                    if (!client.Value.WaitUntilConnected(TimeSpan.FromSeconds(_registryCenterOptions.ConnectionTimeout)))

                    {
                        _zookeeperClients.TryRemove(client.Key, out IZookeeperClient removeClient);
                        removeClient?.Dispose();
                    }
                }
                CreateZookeeperClients();
            };
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
            var random = new Random((int)DateTime.Now.Ticks);
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
            _timer?.Dispose();
            foreach (var _client in _zookeeperClients)
            {
                _client.Value?.Dispose();
            }
        }
    }
}