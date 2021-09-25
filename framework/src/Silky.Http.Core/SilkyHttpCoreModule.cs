using Autofac;
using Microsoft.AspNetCore.Builder;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Http.Core.Handlers;
using Silky.Http.Core.Routing.Builder.Internal;

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
            // builder.RegisterType<ServiceRouteBuilder>();
            //  builder.RegisterType<ServiceEntryCallHandlerFactory>();
            builder.RegisterType<SilkyServiceEntryEndpointDataSource>();
            builder.RegisterType<ServiceEntryEndpointFactory>();
        }
    }
}