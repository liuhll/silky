using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteProvider : ISingletonDependency
    {
        Task RegisterRpcRoutes(ServiceProtocol serviceProtocol);
        Task RegisterWsRoutes(int wsPort);
    }
}