using Silky.Lms.Core;
using Silky.Lms.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Lms.Validation
{
    public class ValidationConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
            services.Configure<LmsValidationOptions>(options =>
            {
                options.ObjectValidationContributors.AddIfNotContains(typeof(DataAnnotationObjectValidationContributor));
            });
        }

        public int Order { get; } = 9999;
    }
}