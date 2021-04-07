using System;
using FluentValidation;
using Silky.Lms.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Core;
using Silky.Lms.Core.Extensions.Collections.Generic;

namespace Silky.Lms.FluentValidation
{
    public class FluentValidationConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LmsValidationOptions>(options =>
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

        public int Order { get; } = 998;
    }
}