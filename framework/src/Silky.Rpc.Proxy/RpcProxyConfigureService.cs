using System;
using Castle.DynamicProxy;
using Silky.Castle.Adapter;
using Silky.Core;
using Silky.Rpc.Proxy.Interceptors;
using Silky.Rpc.Runtime.Server;
using Silky.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Rpc.Proxy
{
    public class RpcProxyConfigureService : IConfigureService
    {
        private static readonly ProxyGenerator ProxyGeneratorInstance = new ProxyGenerator();

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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

        public int Order { get; } = 2;
    }
}