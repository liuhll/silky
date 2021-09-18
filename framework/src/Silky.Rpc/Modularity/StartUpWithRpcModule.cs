using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Modularity
{
    public abstract class StartUpWithRpcModule : StartUpModule
    {
        public override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceRouteProvider = applicationContext.ServiceProvider.GetRequiredService<IServiceRouteProvider>();
            return serviceRouteProvider.EnterRoutes();
        }
    }
}