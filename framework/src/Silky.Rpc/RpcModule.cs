using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Extensions;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc
{
    public class RpcModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RpcOptions>()
                .Bind(configuration.GetSection(RpcOptions.Rpc));
            services.AddOptions<GovernanceOptions>()
                .Bind(configuration.GetSection(GovernanceOptions.Governance));
            services.AddOptions<WebSocketOptions>()
                .Bind(configuration.GetSection(WebSocketOptions.WebSocket));

            services.AddDefaultMessageCodec();
            services.AddAuditing(configuration);
            services.TryAddSingleton<IHeartBeatService, DefaultHeartBeatService>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localServiceTypes = ServiceHelper.FindLocalServiceTypes(EngineContext.Current.TypeFinder)
                .ToArray();
            builder.RegisterTypes(localServiceTypes)
                .PropertiesAutowired()
                .AsSelf()
                .InstancePerLifetimeScope()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localServiceTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
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
            RegisterServicesForParameterResolver(builder);
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            if (!applicationContext.IsAddRegistryCenterService(out var registryCenterType))
            {
                throw new SilkyException(
                    $"You did not specify the dependent {registryCenterType} service registry module");
            }

            var messageListeners = applicationContext.ServiceProvider.GetServices<IServerMessageListener>();
            if (messageListeners.Any())
            {
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {

                        using var serviceScope = EngineContext.Current.ServiceProvider.CreateScope();
                        var rpcContextAccessor = EngineContext.Current.Resolve<IRpcContextAccessor>();
                        rpcContextAccessor.RpcContext = RpcContext.Context;
                        rpcContextAccessor.RpcContext.RpcServices = serviceScope.ServiceProvider;
                        Debug.Assert(message.IsInvokeMessage());
                        message.SetRpcMessageId();
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                        remoteInvokeMessage.SetRpcAttachments();
                        var serverDiagnosticListener = EngineContext.Current.Resolve<IServerDiagnosticListener>();
                        var tracingTimestamp = serverDiagnosticListener.TracingBefore(remoteInvokeMessage, message.Id);
                        var handlePolicyBuilder = EngineContext.Current.Resolve<IHandlePolicyBuilder>();
                        var policy = handlePolicyBuilder.Build(remoteInvokeMessage);
                        var context = new Context(PollyContextNames.ServerHandlerOperationKey);
                        context[PollyContextNames.TracingTimestamp] = tracingTimestamp;
                        var result = await policy.ExecuteAsync(async ct =>
                        {
                            var messageReceivedHandler = EngineContext.Current.Resolve<IServerMessageReceivedHandler>();
                            var remoteResultMessage =
                                await messageReceivedHandler.Handle(remoteInvokeMessage, ct,
                                    CancellationToken.None);
                            return remoteResultMessage;
                        }, context);
                        var resultTransportMessage = new TransportMessage(result, message.Id);
                        await sender.SendMessageAsync(resultTransportMessage);
                        serverDiagnosticListener.TracingAfter(tracingTimestamp, message.Id,
                            remoteInvokeMessage.ServiceEntryId, result);
                    };
                }
            }
        }


        private void RegisterServicesForAddressSelector(ContainerBuilder builder)
        {
            builder.RegisterType<PollingRpcEndpointSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IRpcEndpointSelector>(ShuntStrategy.Polling.ToString());
            builder.RegisterType<PollingRpcEndpointSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IRpcEndpointSelector>(ShuntStrategy.Random.ToString());
            builder.RegisterType<HashAlgorithmRpcEndpointSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IRpcEndpointSelector>(ShuntStrategy.HashAlgorithm.ToString());
        }

        private void RegisterServicesForParameterResolver(ContainerBuilder builder)
        {
            builder.RegisterType<TemplateParameterResolver>()
                .SingleInstance()
                .AsSelf()
                .Named<IParameterResolver>(ParameterType.Dict.ToString());

            builder.RegisterType<RpcParameterResolver>()
                .SingleInstance()
                .AsSelf()
                .Named<IParameterResolver>(ParameterType.Rpc.ToString());

            builder.RegisterType<HttpParameterResolver>()
                .SingleInstance()
                .AsSelf()
                .Named<IParameterResolver>(ParameterType.Http.ToString());
        }

        private void RegisterServicesExecutor(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerDependency()
                ;

            builder.RegisterType<DefaultRemoteExecutor>()
                .As<IRemoteExecutor>()
                .InstancePerDependency()
                ;

            builder.RegisterType<DefaultExecutor>()
                .As<IExecutor>()
                .InstancePerDependency()
                ;
        }
    }
}