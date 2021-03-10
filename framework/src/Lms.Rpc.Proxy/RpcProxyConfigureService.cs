using System;
using Castle.DynamicProxy;
using Lms.Castle.Adapter;
using Lms.Core;
using Lms.Rpc.Proxy.Interceptors;
using Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Rpc.Proxy
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
            var interceptorAdapterType =
                typeof(LmsAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
            services.AddTransient(
                type,
                serviceProvider => ProxyGeneratorInstance
                    .CreateInterfaceProxyWithoutTarget(
                        type,
                        (IInterceptor) serviceProvider.GetRequiredService(interceptorAdapterType)
                       // (IInterceptor) serviceProvider.GetRequiredService(fallbackInterceptorAdapterType)
                    )
            );
        }

        public int Order { get; } = 2;
    }
}