using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;
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

        public override async Task Initialize(ApplicationInitializationContext context)
        {
            var messageListener =
                context.ServiceProvider.GetRequiredService<DotNettyTcpServerMessageListener>();
            await messageListener.Listen();
            var serverProvider =
                context.ServiceProvider.GetRequiredService<IServerProvider>();
            serverProvider.AddRpcServices();
        }
    }
}