using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
using Lms.Rpc.Transport.Codec;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Rpc
{
    public class RpcModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterTypes(
                    ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder).ToArray())
                .AsSelf()
                .AsImplementedInterfaces();
            builder.RegisterType<DefaultTransportMessageEncoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<DefaultTransportMessageDecoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
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
                if (!EngineContext.Current.IsRegistered(typeof(ITransportMessageDecoder))
                    || !EngineContext.Current.IsRegistered(typeof(ITransportMessageEncoder)))
                {
                    throw new LmsException("必须指定消息编解码器");
                }

                var serviceEntryLocate = applicationContext.ServiceProvider.GetService<IServiceEntryLocator>();
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {
                        Debug.Assert(message.IsInvokeMessage());
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                        RpcContext.GetContext().SetAttachments(remoteInvokeMessage.Attachments);
                        var serviceEntry =
                            serviceEntryLocate.GetLocalServiceEntryById(remoteInvokeMessage.ServiceId);
                        RemoteResultMessage remoteResultMessage;
                        try
                        {
                            var result = await serviceEntry.Executor(null, remoteInvokeMessage.Parameters);
                            remoteResultMessage = new RemoteResultMessage()
                            {
                                Result = result,
                                StatusCode = StatusCode.Success
                            };
                        }
                        catch (Exception e)
                        {
                            remoteResultMessage = new RemoteResultMessage()
                            {
                                ExceptionMessage = e.GetExceptionMessage(),
                                StatusCode = e.GetExceptionStatusCode()
                            };
                        }

                        var resultTransportMessage = new TransportMessage(remoteResultMessage, message.Id);
                        await sender.SendAndFlushAsync(resultTransportMessage);
                    };
                }
            }
        }
    }
}