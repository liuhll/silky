using System.Threading.Tasks;
using Autofac;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Extensions;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyHttpCoreModule : HttpSilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<OuterHttpMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(HttpMessageType.Outer.ToString());
            builder.RegisterType<OuterHttpMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(HttpMessageType.Inner.ToString());


            builder.RegisterType<OuterHttpRequestParameterParser>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IParameterParser>(HttpMessageType.Outer.ToString());
        }
    }
}