using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulServiceProvider : IServiceProvider
    {
        private readonly IConsulClientFactory _consulClientFactory;
        private readonly ISerializer _serializer;
        private const string servicePath = "services/{0}";
        private ConcurrentDictionary<string, ServiceDescriptor[]> servicesCache = new();

        public ConsulServiceProvider(IConsulClientFactory consulClientFactory,
            ISerializer serializer)
        {
            _consulClientFactory = consulClientFactory;
            _serializer = serializer;
        }

        public async Task PublishAsync(string serverName, ServiceDescriptor[] serviceDescriptors)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var serviceJsonString = _serializer.Serialize(serviceDescriptors);
            var servicesPutResult = await consulClient.KV.Put(new KVPair(string.Format(servicePath, serverName))
            {
                Value = serviceJsonString.GetBytes()
            });
            if (servicesPutResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Register Server To ServiceCenter Consul Error");
            }
        }

        public async Task<ServiceDescriptor[]> GetServices(string serverName)
        {
            if (servicesCache.TryGetValue(serverName, out var serverDescriptors))
            {
                return serverDescriptors;
            }

            using var consulClient = _consulClientFactory.CreateClient();
            var getKvResult = await consulClient.KV.Get(string.Format(servicePath, serverName));
            if (getKvResult.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<ServiceDescriptor>();
            }

            if (getKvResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Get services from consul error");
            }

            var serviceJsonString = getKvResult.Response.Value.GetString();
            serverDescriptors = _serializer.Deserialize<ServiceDescriptor[]>(serviceJsonString);
            servicesCache.TryAdd(serverName, serverDescriptors);
            return serverDescriptors;
        }
    }
}