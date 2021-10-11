using System.Linq;
using AutoMapper;
using Silky.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoMapperServiceCollectionExtensions
    {
        public static IServiceCollection AddObjectMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                var profileTypes = EngineContext.Current.TypeFinder.FindClassesOfType<Profile>()
                    .Where(p => !p.IsAbstract);

                foreach (var profileType in profileTypes)
                {
                    cfg.AddProfile(profileType);
                }
            });
            var mapper = config.CreateMapper();
            services.AddSingleton<IMapper>(mapper);
            return services;
        }
    }
}