using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Threading;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServerRegisterProvider : IListener, IServerRegisterProvider
    {
        private readonly INacosConfigService _nacosConfigService;
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;
        private readonly ISerializer _serializer;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly IServiceProvider _serviceProvider;

        public NacosServerRegisterProvider(INacosConfigService nacosConfigService,
            IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions,
            ISerializer serializer,
            IDistributedLockProvider distributedLockProvider,
            IServiceProvider serviceProvider)
        {
            _nacosConfigService = nacosConfigService;
            _serializer = serializer;
            _distributedLockProvider = distributedLockProvider;
            _serviceProvider = serviceProvider;
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
        }

        public async Task AddServer()
        {
            await using (await _distributedLockProvider.AcquireLockAsync("AddServerNameToNacos"))
            {
                await _nacosConfigService.AddListener(_nacosRegistryCenterOptions.ServerKey,
                    _nacosRegistryCenterOptions.ServerGroupName, this);
                var servers = await GetAllServerNames();
                if (servers.Contains(EngineContext.Current.HostName))
                {
                    return;
                }

                servers.Add(EngineContext.Current.HostName);
                var registerServersValue = _serializer.Serialize(servers);
                var result = await _nacosConfigService.PublishConfig(_nacosRegistryCenterOptions.ServerKey,
                    _nacosRegistryCenterOptions.ServerGroupName,
                    registerServersValue);
                if (!result)
                {
                    throw new SilkyException($"{EngineContext.Current.HostName} registration failed");
                }
            }
        }

        public async Task<IList<string>> GetAllServerNames(int timeoutMs = 10000)
        {
            var registerServersValue =
                await _nacosConfigService.GetConfig(_nacosRegistryCenterOptions.ServerKey,
                    _nacosRegistryCenterOptions.ServerGroupName, timeoutMs);
            if (registerServersValue.IsNullOrEmpty())
            {
                return new List<string>();
            }

            return _serializer.Deserialize<IList<string>>(registerServersValue);
        }

        public async Task<ServerDescriptor> GetServerDescriptor(string serverName, List<Instance> serverInstances)
        {
            var serverDescriptor = new ServerDescriptor()
            {
                HostName = serverName,
                Services = await _serviceProvider.GetServices(serverName),
            };
            var endpoints = new List<SilkyEndpointDescriptor>();
            foreach (var instance in serverInstances)
            {
                var instanceEndpoints = instance.GetEndpoints();
                endpoints.AddRange(instanceEndpoints);
            }

            serverDescriptor.Endpoints = endpoints.ToArray();
            return serverDescriptor;
        }

        public async void ReceiveConfigInfo(string configInfo)
        {
            if (configInfo.IsNullOrEmpty())
            {
                return;
            }

            var serverNames = _serializer.Deserialize<string[]>(configInfo);
            var nacosServerRegister = EngineContext.Current.Resolve<IServerRegister>() as NacosServerRegister;
            Debug.Assert(nacosServerRegister != null, "nacosServerRegister != null");
            foreach (var server in serverNames)
            {
                await nacosServerRegister.CreateServerListener(server);
            }
        }
    }
}