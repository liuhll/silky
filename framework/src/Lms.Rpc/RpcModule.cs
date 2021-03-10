using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Lms.Castle;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Address;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Interceptors;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;
using Lms.Rpc.Transport;
using Lms.Rpc.Transport.Codec;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Rpc
{
    public class RpcModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localEntryTypes = ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder)
                .ToArray();
            builder.RegisterTypes(localEntryTypes)
                .PropertiesAutowired()
                .AsSelf()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localEntryTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
            foreach (var serviceKeyType in serviceKeyTypes)
            {
                var serviceKeyAttribute = serviceKeyType.GetCustomAttributes().OfType<ServiceKeyAttribute>().First();
                builder.RegisterType(serviceKeyType).Named(serviceKeyAttribute.Name,
                        serviceKeyType.GetInterfaces().First(p =>
                            p.GetCustomAttributes().OfType<IRouteTemplateProvider>().Any()))
                    ;
            }

            RegisterServicesForAddressSelector(builder);
            RegisterServicesExecutor(builder);
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceRouteManager = applicationContext.ServiceProvider.GetService<IServiceRouteManager>();
            if (serviceRouteManager == null)
            {
                throw new LmsException("您必须指定依赖的服务注册中心模块");
            }

            await serviceRouteManager.CreateSubscribeDataChanges();
            await serviceRouteManager.EnterRoutes(ServiceProtocol.Tcp);
            var messageListeners = applicationContext.ServiceProvider.GetServices<IServerMessageListener>();
            if (messageListeners.Any())
            {
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {
                        Debug.Assert(message.IsInvokeMessage());
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                        var messageReceivedHandler = EngineContext.Current.Resolve<IServiceMessageReceivedHandler>();
                        await messageReceivedHandler.Handle(message.Id, sender, remoteInvokeMessage);
                    };
                }
            }
        }


        private void RegisterServicesForAddressSelector(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultTransportMessageEncoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();
            builder.RegisterType<DefaultTransportMessageDecoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();

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
                .AddInterceptors(
                    typeof(TransactionInterceptor));

            builder.RegisterType<DefaultRemoteServiceExecutor>()
                .As<IRemoteServiceExecutor>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DefaultServiceExecutor>()
                .As<IServiceExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(CachingInterceptor));
        }
    }
}