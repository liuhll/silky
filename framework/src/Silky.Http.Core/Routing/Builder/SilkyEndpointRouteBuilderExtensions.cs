using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core.Routing.Builder.Internal;
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

            var hostApplicationLifetime = endpoints.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await RegisterSilkyWebServer(endpoints.ServiceProvider);
            });
            return GetOrCreateServiceEntryDataSource(endpoints).DefaultBuilder;
        }

        public static ServiceEntryDescriptorEndpointConventionBuilder MapSilkyTemplateServices(
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
            ;
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