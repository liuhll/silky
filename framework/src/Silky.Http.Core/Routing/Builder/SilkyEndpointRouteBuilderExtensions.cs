using System;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Core.Routing.Builder.Internal;

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

            return GetOrCreateServiceEntryDataSource(endpoints).DefaultBuilder;
        }
        
        public static ServiceEntryDescriptorEndpointConventionBuilder MapSilkyTemplateServices(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            return GetOrCreateServiceEntryDescriptorDataSource(endpoints).DefaultBuilder;
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