using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Modularity
{
    public abstract class StartUpWithRpcModule : StartUpModule
    {
        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var serverRouteRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRouteRegister>();
            await serverRouteRegister.RegisterServerRoute();
        }
    }
}