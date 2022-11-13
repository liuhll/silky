using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Http.Core.Routing.Builder.Internal;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.AspNetCore.Builder
{
    public static class SilkyEndpointRouteBuilderExtensions
    {
        public static SilkyRpcServiceEndpointConventionBuilder MapSilkyServiceEntries(
            this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var hostApplicationLifetime = endpoints.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await RegisterSilkyWebServer(endpoints.ServiceProvider);
            });
            return GetOrCreateServiceEntryDataSource(endpoints).DefaultBuilder;
        }

        public static SilkyRpcServiceEndpointConventionBuilder MapSilkyRpcServices(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var hostApplicationLifetime = endpoints.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await RegisterSilkyWebServer(endpoints.ServiceProvider);
            });
            return GetOrCreateSilkyRpcServiceDataSource(endpoints).DefaultBuilder;
        }

        public static SilkyRpcServiceEndpointConventionBuilder MapSilkyTemplateServices(
            this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var hostApplicationLifetime = endpoints.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await RegisterSilkyWebServer(endpoints.ServiceProvider);
            });
            return GetOrCreateServiceEntryDescriptorDataSource(endpoints).DefaultBuilder;
        }


        public static SilkyRpcServiceEndpointConventionBuilder MapSilkyDashboardServices(
            this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            return GetOrCreateSilkyDashboardDataSource(endpoints).DefaultBuilder;
        }

        private static SilkyDashboardEndpointDataSource GetOrCreateSilkyDashboardDataSource(
            IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.DataSources.OfType<SilkyDashboardEndpointDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = endpoints.ServiceProvider.GetRequiredService<SilkyDashboardEndpointDataSource>();
                endpoints.DataSources.Add(dataSource);
            }

            return dataSource;
        }


        private static async Task RegisterSilkyWebServer(IServiceProvider serviceProvider)
        {
            var serverProvider =
                serviceProvider.GetRequiredService<IServerProvider>();
            serverProvider.AddHttpServices();
        }

        private static SilkyServiceEntryEndpointDataSource GetOrCreateServiceEntryDataSource(
            IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.DataSources.OfType<SilkyServiceEntryEndpointDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = endpoints.ServiceProvider.GetRequiredService<SilkyServiceEntryEndpointDataSource>();
                endpoints.DataSources.AddIfNotContains(dataSource);
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

        private static SilkyRpcServiceEndpointDataSource GetOrCreateSilkyRpcServiceDataSource(
            IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.DataSources.OfType<SilkyRpcServiceEndpointDataSource>()
                .FirstOrDefault();
            if (dataSource == null)
            {
                dataSource =
                    endpoints.ServiceProvider.GetRequiredService<SilkyRpcServiceEndpointDataSource>();
                endpoints.DataSources.Add(dataSource);
            }

            return dataSource;
        }
    }
}