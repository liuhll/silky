using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Silky.Core.Exceptions.ValidationException;

namespace Silky.Validation
{
    public class ObjectValidator : IObjectValidator, ITransientDependency
    {
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected SilkyValidationOptions Options { get; }

        public ObjectValidator(IOptions<SilkyValidationOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
            Options = options.Value;
        }

        public virtual void Validate(object validatingObject, string name = null, bool allowNull = false)
        {
            var errors = GetErrors(validatingObject, name, allowNull);

            if (errors.Any())
            {
                throw new ValidationException(
                    "Input parameter is invalid",
                    errors
                );
            }
        }

        public virtual List<ValidationResult> GetErrors(object validatingObject, string name = null,
            bool allowNull = false)
        {
            if (validatingObject == null)
            {
                if (allowNull)
                {
                    return new List<ValidationResult>(); //TODO: Returning an array would be more performent
                }
                else
                {
                    return new List<ValidationResult>
                    {
                        name == null
                            ? new ValidationResult("Given object is null!")
                            : new ValidationResult(name + " is null!", new[] { name })
                    };
                }
            }

            var context = new ObjectValidationContext(validatingObject);

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                foreach (var contributorType in Options.ObjectValidationContributors)
                {
                    var contributor = (IObjectValidationContributor)
                        scope.ServiceProvider.GetRequiredService(contributorType);
                    contributor.AddErrors(context);
                }
            }

            return context.Errors;
        }
    }
}