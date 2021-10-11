using System.Linq;
using Mapster;
using MapsterMapper;
using Silky.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MapsterServiceCollectionExtensions
    {
        public static IServiceCollection AddObjectMapper(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            var assemblies = EngineContext.Current.TypeFinder.FindClassesOfType<IRegister>().Select(p => p.Assembly)
                .Distinct().ToArray();
            if (assemblies.Any())
            {
                config.Scan(assemblies);
            }

            config.Default
                .NameMatchingStrategy(NameMatchingStrategy.Flexible)
                .PreserveReference(true);
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
            return services;
        }
    }
}