using Lms.Castle.Adapter;
using Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Castle
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