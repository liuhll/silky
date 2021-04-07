using System;
using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Routing
{
    public interface IServiceRouteProvider : ISingletonDependency
    {
        Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol);
        Task RegisterWsRoutes(double processorTime, Type[] wsAppServiceTypes, int wsPort);
    }
}