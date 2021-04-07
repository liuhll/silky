using System;
using Castle.DynamicProxy;
using Silky.Lms.Castle.Adapter;
using Silky.Lms.Core;
using Silky.Lms.Rpc.Proxy.Interceptors;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Lms.Rpc.Proxy
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
                typeof(LmsAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
            var validationInterceptorType = typeof(ValidationInterceptor);
            var validationInterceptorAdapterType =
                typeof(LmsAsyncDeterminationInterceptor<>).MakeGenericType(validationInterceptorType);
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