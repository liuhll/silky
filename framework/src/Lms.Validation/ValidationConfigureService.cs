using Lms.Core;
using Lms.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Validation
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