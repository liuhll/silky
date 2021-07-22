using System;
using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle;
using Silky.Castle.Adapter;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Rpc.Proxy.Interceptors;
using Silky.Rpc.Runtime.Server;
using Silky.Validation;

namespace Silky.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : SilkyModule
    {
        private static readonly ProxyGenerator ProxyGeneratorInstance = new();

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var serviceEntryServiceTypes =
                ServiceEntryHelper.FindServiceEntryProxyTypes(EngineContext.Current.TypeFinder);
            foreach (var serviceEntryServiceType in serviceEntryServiceTypes)
            {
                AddAServiceEntryClientProxy(services, serviceEntryServiceType);
            }
        }

        private void AddAServiceEntryClientProxy(IServiceCollection services, Type type)
        {
            var rpcProxyInterceptorType = typeof(RpcClientProxyInterceptor);
            services.AddTransient(rpcProxyInterceptorType);
            var rpcInterceptorAdapterType =
                typeof(SilkyAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
            var validationInterceptorType = typeof(ValidationInterceptor);
            var validationInterceptorAdapterType =
                typeof(SilkyAsyncDeterminationInterceptor<>).MakeGenericType(validationInterceptorType);
            services.AddTransient(
                type,
                serviceProvider => ProxyGeneratorInstance
                    .CreateInterfaceProxyWithoutTarget(
                        type,
                        (IInterceptor) serviceProvider.GetRequiredService(validationInterceptorAdapterType),
                        (IInterceptor) serviceProvider.GetRequiredService(rpcInterceptorAdapterType)
                    )
            );
        }
    }
}