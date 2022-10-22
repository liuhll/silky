using System.Linq;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServerExtensions
    {
        public static ServerDescriptor ConvertToDescriptor(this IServer server)
        {
            var descriptor = new ServerDescriptor()
            {
                HostName = server.HostName,
                Services = server.Services.ToArray(),
                Endpoints = server.Endpoints.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }

        public static bool HasHttpProtocolServiceEntry(this IServer server)
        {
            var serviceEntries = server.Services.SelectMany(s => s.ServiceEntries);
            return serviceEntries.Any(p => p.GetMetadata<bool>(ServiceEntryConstant.NeedHttpProtocolSupport));
        }
    }
}