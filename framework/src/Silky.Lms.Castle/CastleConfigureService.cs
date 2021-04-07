using Silky.Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Castle.Adapter;

namespace Silky.Lms.Castle
{
    public class CastleConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(LmsAsyncDeterminationInterceptor<>));

        }

        public int Order { get; } = 1;
    }
}