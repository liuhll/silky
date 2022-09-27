using System;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Http.Core;
using Silky.Http.Core.Routing.Builder.Internal;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.AspNetCore.Builder
{
    public static class SilkyEndpointRouteBuilderExtensions
    {
        public static ServiceEntryEndpointConventionBuilder MapSilkyRpcServices(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            RegisterSilkyWebServer(endpoints.ServiceProvider);
            return GetOrCreateServiceEntryDataSource(endpoints).DefaultBuilder;
        }

        public static ServiceEntryDescriptorEndpointConventionBuilder MapSilkyTemplateServices(
            this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            RegisterSilkyWebServer(endpoints.ServiceProvider);
            return GetOrCreateServiceEntryDescriptorDataSource(endpoints).DefaultBuilder;
        }


        private static void RegisterSilkyWebServer(IServiceProvider serviceProvider)
        {
            var serverRegisterProvider =
                serviceProvider.GetRequiredService<IServerProvider>();
            var serverRouteRegister =
                serviceProvider.GetRequiredService<IServerRegister>();

            serverRegisterProvider.AddHttpServices();
            serverRouteRegister.RegisterServer().GetAwaiter().GetResult();

            var invokeMonitor =
                serviceProvider.GetService<IInvokeMonitor>();
            invokeMonitor?.ClearCache().GetAwaiter().GetResult();

            var serverHandleMonitor =
                serviceProvider.GetService<IServerHandleMonitor>();
            serverHandleMonitor?.ClearCache().GetAwaiter().GetResult();
        }

        private static SilkyServiceEntryEndpointDataSource GetOrCreateServiceEntryDataSource(
            IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.DataSources.OfType<SilkyServiceEntryEndpointDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = endpoints.ServiceProvider.GetRequiredService<SilkyServiceEntryEndpointDataSource>();
                endpoints.DataSources.Add(dataSource);
            }

            return dataSource;
        }

        private static SilkyServiceEntryDescriptorEndpointDataSource GetOrCreateServiceEntryDescriptorDataSource(
            IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.DataSources.OfType<SilkyServiceEntryDescriptorEndpointDataSource>()
                .FirstOrDefault();
            if (dataSource == null)
            {
                dataSource =
                    endpoints.ServiceProvider.GetRequiredService<SilkyServiceEntryDescriptorEndpointDataSource>();
                endpoints.DataSources.Add(dataSource);
            }

            return dataSource;
        }
    }
}