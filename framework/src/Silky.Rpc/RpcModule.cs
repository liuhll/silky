using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Extensions;
using Silky.Rpc.Transport.Codec;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc
{
    public class RpcModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RpcOptions>()
                .Bind(configuration.GetSection(RpcOptions.Rpc));
            services.AddOptions<RegistryCenterOptions>()
                .Bind(configuration.GetSection(RegistryCenterOptions.RegistryCenter));
            services.AddOptions<GovernanceOptions>()
                .Bind(configuration.GetSection(GovernanceOptions.Governance));
            services.AddOptions<WebSocketOptions>()
                .Bind(configuration.GetSection(WebSocketOptions.WebSocket));
            if (!services.IsAdded(typeof(ITransportMessageDecoder)))
            {
                services.AddTransient<ITransportMessageDecoder, DefaultTransportMessageDecoder>();
            }
            if (!services.IsAdded(typeof(ITransportMessageEncoder)))
            {
                services.AddTransient<ITransportMessageEncoder, DefaultTransportMessageEncoder>();
            }
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localEntryTypes = ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder)
                .ToArray();
            builder.RegisterTypes(localEntryTypes)
                .PropertiesAutowired()
                .AsSelf()
                .InstancePerLifetimeScope()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localEntryTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
            foreach (var serviceKeyType in serviceKeyTypes)
            {
                var serviceKeyAttribute = serviceKeyType.GetCustomAttributes().OfType<ServiceKeyAttribute>().First();
                builder.RegisterType(serviceKeyType)
                    .Named(serviceKeyAttribute.Name,
                        serviceKeyType.GetInterfaces().First(p =>
                            p.GetCustomAttributes().OfType<IRouteTemplateProvider>().Any()))
                    .InstancePerLifetimeScope()
                    .AsImplementedInterfaces()
                    ;
            }

            RegisterServicesForAddressSelector(builder);
            RegisterServicesExecutor(builder);
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            if (!applicationContext.IsDependsOnRegistryCenterModule(out var registryCenterType))
            {
                throw new SilkyException(
                    $"You did not specify the dependent {registryCenterType} service registry module");
            }

            var serviceRouteManager = applicationContext.ServiceProvider.GetRequiredService<IServiceRouteManager>();
            await serviceRouteManager.EnterRoutes();
            var messageListeners = applicationContext.ServiceProvider.GetServices<IServerMessageListener>();
            if (messageListeners.Any())
            {
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {
                        using var rpcContextAccessor = EngineContext.Current.Resolve<IRpcContextAccessor>();
                        rpcContextAccessor.RpcContext = RpcContext.Context;
                        Debug.Assert(message.IsInvokeMessage());
                        message.SetRpcMessageId();
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                        var messageReceivedHandler = EngineContext.Current.Resolve<IServerMessageReceivedHandler>();
                        await messageReceivedHandler.Handle(message.Id, sender, remoteInvokeMessage);
                    };
                }
            }
        }


        private void RegisterServicesForAddressSelector(ContainerBuilder builder)
        {
            builder.RegisterType<PollingAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.Polling.ToString());
            builder.RegisterType<PollingAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.Random.ToString());
            builder.RegisterType<HashAlgorithmAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
        }

        private void RegisterServicesExecutor(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerLifetimeScope()
                ;

            builder.RegisterType<DefaultRemoteExecutor>()
                .As<IRemoteExecutor>()
                .InstancePerLifetimeScope()
                ;

            builder.RegisterType<DefaultExecutor>()
                .As<IExecutor>()
                .InstancePerLifetimeScope()
                ;
        }
    }
}