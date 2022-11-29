using System;
using System.Collections.Generic;
using System.Linq;
using Consul;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    internal static class ServerDescriptorExtensions
    {
        public static AgentServiceRegistration CreateAgentServiceRegistration(this ServerDescriptor serverDescriptor,
            ConsulRegistryCenterOptions consulRegistryCenterOptions)
        {
            var endpoint = serverDescriptor.GetRegisterEndpoint();
            var endpoints = new Dictionary<ServiceProtocol, string>();
            foreach (var rpcEndpointDescriptor in serverDescriptor.Endpoints)
            {
                endpoints[rpcEndpointDescriptor.ServiceProtocol] = rpcEndpointDescriptor.GetAddress();
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var endpointsJsonString = serializer.Serialize(endpoints);

            var agentServiceRegistration = new AgentServiceRegistration()
            {
                ID = endpoint.GetAddress(),
                Address = endpoint.Host,
                Port = endpoint.Port,
                Name = serverDescriptor.HostName,
                EnableTagOverride = false,
                Meta = new Dictionary<string, string>()
                {
                    { "ProcessorTime", endpoint.ProcessorTime.ToString() },
                    { "TimeStamp", endpoint.TimeStamp.ToString() },
                    { "Endpoints", endpointsJsonString },
                    { "HostName", EngineContext.Current.HostName },
                    { "HealthCheck", consulRegistryCenterOptions.HealthCheck.ToString() }
                },
                Tags = new[]
                {
                    EngineContext.Current.HostName,
                    ConsulRegistryCenterOptions.SilkyServer
                },
            };

            if (consulRegistryCenterOptions.HealthCheck)
            {
                agentServiceRegistration.Check = new AgentServiceCheck()
                {
                    Name = $"{serverDescriptor.HostName} - {endpoint.GetAddress()}",
                    TCP = endpoint.GetAddress(),
                    Interval = TimeSpan.FromSeconds(consulRegistryCenterOptions.HealthCheckIntervalSecond),
                    Timeout = TimeSpan.FromSeconds(consulRegistryCenterOptions.HealthCheckTimeoutSecond),
                    Notes = $"Check {serverDescriptor.HostName} {endpoint.GetAddress()} health through tcl protocol "
                };
            }


            return agentServiceRegistration;
        }


        public static string GetInstanceId(this IServer server)
        {
            var endpoint = server.Endpoints.FirstOrDefault(p =>
                p.ServiceProtocol == ServiceProtocol.Rpc || p.ServiceProtocol.IsHttp());

            if (endpoint == null)
            {
                throw new SilkyException("RpcEndpoint does not exist");
            }

            return endpoint.Descriptor.ToString();
        }
    }
}