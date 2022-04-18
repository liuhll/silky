using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Handlers;
using Silky.Http.Core.Routing.Builder.Internal;

namespace Silky.Http.Core
{
    public static class SilkyHttpCoreCollectionExtensions
    {
        public static IServiceCollection AddSilkyHttpCore(this IServiceCollection services)
        {
            services.AddSingleton<IMessageReceivedHandler, DefaultHttpMessageReceivedHandler>();
            services.AddSingleton<IParameterParser, DefaultHttpRequestParameterParser>();
            services.AddSingleton<SilkyServiceEntryEndpointDataSource>();
            services.AddSingleton<ServiceEntryEndpointFactory>();

            services.AddSingleton<SilkyServiceEntryDescriptorEndpointDataSource>();
            services.AddSingleton<ServiceEntryDescriptorEndpointFactory>();

            services.AddOptions<GatewayOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(GatewayOptions.Gateway));
            services.AddHttpContextAccessor();
            return services;
        }
    }
}