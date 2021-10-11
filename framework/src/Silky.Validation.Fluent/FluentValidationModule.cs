using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Modularity;

namespace Silky.Validation.Fluent
{
    [DependsOn(typeof(ValidationModule))]
    public class FluentValidationModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SilkyValidationOptions>(options =>
            {
                options.ObjectValidationContributors.AddIfNotContains(typeof(FluentObjectValidationContributor));
            });
            var validatorTypes = EngineContext.Current.TypeFinder.FindClassesOfType(typeof(IValidator));
            foreach (var validatorType in validatorTypes)
            {
                var validatingType = GetFirstGenericArgumentOrNull(validatorType, 1);
                if (validatingType == null)
                {
                    continue;
                }

                var serviceType = typeof(IValidator<>).MakeGenericType(validatingType);
                services.AddTransient(
                    serviceType,
                    validatorType
                );
            }
        }


        private static Type GetFirstGenericArgumentOrNull(Type type, int depth)
        {
            const int maxFindDepth = 8;

            if (depth >= maxFindDepth)
            {
                return null;
            }

            if (type.IsGenericType && type.GetGenericArguments().Length >= 1)
            {
                return type.GetGenericArguments()[0];
            }

            return GetFirstGenericArgumentOrNull(type.BaseType, depth + 1);
        }
    }
}