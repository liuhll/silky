using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace Silky.DotNetty.Protocol.Tcp
{
    [DependsOn(typeof(RpcModule), typeof(DotNettyModule))]
    public class DotNettyTcpModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DotNettyTcpServerMessageListener>()
                .AsSelf()
                .SingleInstance()
                .AsImplementedInterfaces()
                .PropertiesAutowired();
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var messageListener =
                applicationContext.ServiceProvider.GetRequiredService<DotNettyTcpServerMessageListener>();
            await messageListener.Listen();
            var serverRegisterProvider =
                applicationContext.ServiceProvider.GetRequiredService<IServerProvider>();
            serverRegisterProvider.AddTcpServices();
        }
    }
}