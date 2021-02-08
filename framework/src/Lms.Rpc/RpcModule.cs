using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
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
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceRouteManager = applicationContext.ServiceProvider.GetService<IServiceRouteManager>();
            if (serviceRouteManager != null)
            {
                await serviceRouteManager.EnterRoutes();
            }

            var messageListeners = applicationContext.ServiceProvider.GetServices<IMessageListener>();
            if (messageListeners.Any())
            {
                var serviceEntryLocate = applicationContext.ServiceProvider.GetService<IServiceEntryLocate>();
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {
                        Debug.Assert(message.IsInvokeMessage());
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
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

                        var resultTransportMessage = new TransportMessage(remoteResultMessage);
                        await sender.SendAndFlushAsync(resultTransportMessage);
                    };
                }
            }
        }
        
    }
}