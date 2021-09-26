using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServerRegisterProvider : IListener, IServerRegisterProvider
    {
        private readonly INacosConfigService _nacosConfigService;
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;
        private readonly ISerializer _serializer;

        private const string silky_servers = "silky_servers";


        public NacosServerRegisterProvider(INacosConfigService nacosConfigService,
            IOptionsMonitor<NacosRegistryCenterOptions> nacosRegistryCenterOptions,
            ISerializer serializer)
        {
            _nacosConfigService = nacosConfigService;
            _serializer = serializer;
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.CurrentValue;
        }

        public async Task AddServer()
        {
            await _nacosConfigService.AddListener(silky_servers, _nacosRegistryCenterOptions.GroupName, this);
            var servers = await GetAllServerNames();
            if (servers.Contains(EngineContext.Current.HostName))
            {
                return;
            }

            var registerServers = servers.Concat(new[] { EngineContext.Current.HostName });
            var registerServersValue = _serializer.Serialize(registerServers);
            var result = await _nacosConfigService.PublishConfig(silky_servers, _nacosRegistryCenterOptions.GroupName,
                registerServersValue);
            if (!result)
            {
                throw new SilkyException($"{EngineContext.Current.HostName} registration failed");
            }
        }

        public async Task<string[]> GetAllServerNames(int timeoutMs = 5000)
        {
            var registerServersValue =
                await _nacosConfigService.GetConfig(silky_servers, _nacosRegistryCenterOptions.GroupName, timeoutMs);
            if (registerServersValue.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            return _serializer.Deserialize<string[]>(registerServersValue);
        }

        public ServerDescriptor GetServerDescriptor(string serverName, List<Instance> serverInstances)
        {
            var serverDescriptor = new ServerDescriptor()
            {
                HostName = serverName,
            };
            var endpoints = new List<RpcEndpointDescriptor>();
            foreach (var instance in serverInstances)
            {
                var instanceEndpoints = instance.GetEndpoints();
                endpoints.AddRange(instanceEndpoints);
                serverDescriptor.Services ??=
                    _serializer.Deserialize<ServiceDescriptor[]>(instance.Metadata["Services"]);
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