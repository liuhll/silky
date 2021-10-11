using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Modularity;

namespace Silky.Validation
{
    public class ValidationModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SilkyValidationOptions>(options =>
            {
                options.ObjectValidationContributors.AddIfNotContains(
                    typeof(DataAnnotationObjectValidationContributor));
            });
        }
    }
}