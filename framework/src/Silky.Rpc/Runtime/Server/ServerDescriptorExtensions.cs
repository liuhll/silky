using System.Linq;
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
    }
}