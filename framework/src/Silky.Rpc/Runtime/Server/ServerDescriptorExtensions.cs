using System.Linq;
using Silky.Core.Exceptions;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServerDescriptorExtensions
    {
        public static IServer ConvertToServer(this ServerDescriptor serverDescriptor)
        {
            var server = new Server(serverDescriptor.HostName)
            {
                Services = serverDescriptor.Services.ToArray(),
                Endpoints = serverDescriptor.Endpoints.Select(p => p.ConvertToSilkyEndpoint()).ToArray()
            };
            return server;
        }
        
        public static SilkyEndpointDescriptor GetRegisterEndpoint(this ServerDescriptor serverDescriptor)
        {
            var endpoint = serverDescriptor
                .Endpoints
                .OrderBy(p => p.ServiceProtocol)
                .FirstOrDefault();

            if (endpoint == null)
            {
                throw new SilkyException("RpcEndpoint does not exist");
            }

            return endpoint;
        }
    }
}