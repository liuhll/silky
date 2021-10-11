using System;
using Castle.DynamicProxy;
using Silky.Castle.Adapter;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Proxy;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcProxyCollectionExtensions
    {
        private static readonly ProxyGenerator ProxyGeneratorInstance = new();

        public static IServiceCollection AddRpcProxy(this IServiceCollection services)
        {
            var serviceProxyTypes =
                ServiceHelper.FindServiceProxyTypes(EngineContext.Current.TypeFinder);
            foreach (var serviceServiceType in serviceProxyTypes)
            {
                AddAServiceClientProxy(services, serviceServiceType);
            }

            return services;
        }

        public static IServiceCollection AddRpcProxy(this IServiceCollection services, params Type[] serviceTypes)
        {
            services.AddRpcProxy();
            foreach (var serviceType in serviceTypes)
            {
                if (!serviceType.IsInterface)
                {
                    throw new SilkyException("Only allow to generate Rpc proxy for the interface");
                }

                services.AddAServiceClientProxy(serviceType);
            }

            return services;
        }

        private static void AddAServiceClientProxy(this IServiceCollection services, Type type)
        {
            var rpcProxyInterceptorType = typeof(RpcClientProxyInterceptor);
            services.AddTransient(rpcProxyInterceptorType);
            var rpcInterceptorAdapterType =
                typeof(SilkyAsyncDeterminationInterceptor<>).MakeGenericType(rpcProxyInterceptorType);
            services.AddTransient(
                type,
                serviceProvider => ProxyGeneratorInstance
                    .CreateInterfaceProxyWithoutTarget(
                        type,
                        (IInterceptor)serviceProvider.GetRequiredService(rpcInterceptorAdapterType)
                    )
            );
        }
    }
}