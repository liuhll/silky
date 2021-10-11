using System;
using System.Linq;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Silky.Core.DependencyInjection;
using Silky.Validation;

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

        public void AddErrors(ObjectValidationContext context)
        {
            var serviceType = typeof(IValidator<>).MakeGenericType(context.ValidatingObject.GetType());
            var validator = _serviceProvider.GetService(serviceType) as IValidator;
            if (validator == null)
            {
                return;
            }

            var result = validator.Validate((IValidationContext)Activator.CreateInstance(
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