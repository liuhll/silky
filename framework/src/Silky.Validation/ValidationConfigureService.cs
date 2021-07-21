using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Validation
{
    public class ValidationConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
            services.Configure<SilkyValidationOptions>(options =>
            {
                options.ObjectValidationContributors.AddIfNotContains(typeof(DataAnnotationObjectValidationContributor));
            });
        }

        public int Order { get; } = 9999;
    }
}