using System;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulServiceDescriptorProvider : IServiceDescriptorProvider
    {
        private readonly IConsulClientFactory _consulClientFactory;
        private readonly ISerializer _serializer;
        private readonly ConsulRegistryCenterOptions _consulRegistryCenterOptions;
        
        public ConsulServiceDescriptorProvider(IConsulClientFactory consulClientFactory,
            ISerializer serializer, IOptions<ConsulRegistryCenterOptions> consulRegistryCenterOptions)
        {
            _consulClientFactory = consulClientFactory;
            _serializer = serializer;
            _consulRegistryCenterOptions = consulRegistryCenterOptions.Value;
        }

        public async Task PublishAsync(string serverName, ServiceDescriptor[] serviceDescriptors)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var serviceJsonString = _serializer.Serialize(serviceDescriptors);
            var servicesPutResult = await consulClient.KV.Put(
                new KVPair(CreateServicePath(_consulRegistryCenterOptions.RoutePath, serverName))
                {
                    Value = serviceJsonString.GetBytes()
                });
            if (servicesPutResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException($"Register {serverName} Services To ServiceCenter Consul Error");
            }
        }

        public async Task<ServiceDescriptor[]> GetServices(string serverName)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var getKvResult =
                await consulClient.KV.Get(CreateServicePath(_consulRegistryCenterOptions.RoutePath, serverName));
            if (getKvResult.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<ServiceDescriptor>();
            }

            if (getKvResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Get services from consul error");
            }

            var serviceJsonString = getKvResult.Response.Value.GetString();
            var serverDescriptors = _serializer.Deserialize<ServiceDescriptor[]>(serviceJsonString);
            return serverDescriptors;
        }

        private string CreateServicePath(string basePath, string child)
        {
            var servicePath = basePath;
            if (!servicePath.EndsWith("/"))
            {
                servicePath += "/";
            }

            servicePath += child;
            return servicePath;
        }
    }
}