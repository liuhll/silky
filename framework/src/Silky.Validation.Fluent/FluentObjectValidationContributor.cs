using System;
using System.Linq;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace Silky.Validation.Fluent
{
    public class FluentObjectValidationContributor : IObjectValidationContributor, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentObjectValidationContributor(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddErrors(ObjectValidationContext context)
        {
            var serviceType = typeof(IValidator<>).MakeGenericType(context.ValidatingObject.GetType());
            var validator = _serviceProvider.GetService(serviceType) as IValidator;
            if (validator == null)
            {
                return;
            }

            var result = await validator.ValidateAsync((IValidationContext)Activator.CreateInstance(
                typeof(ValidationContext<>).MakeGenericType(context.ValidatingObject.GetType()),
                context.ValidatingObject));

            if (!result.IsValid)
            {
                context.Errors.AddRange(
                    result.Errors.Select(
                        error =>
                            new ValidationResult(error.ErrorMessage, new[] { error.PropertyName })
                    )
                );
            }
        }
    }
}