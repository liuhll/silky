using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Silky.Lms.Core.Exceptions.ValidationException;

namespace Silky.Lms.Validation
{
    public class ObjectValidator : IObjectValidator, ITransientDependency
    {
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected LmsValidationOptions Options { get; }

        public ObjectValidator(IOptions<LmsValidationOptions> options, IServiceScopeFactory serviceScopeFactory)
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
                    "输入参数校验未通过",
                    errors
                );
            }
        }

        public virtual List<ValidationResult> GetErrors(object validatingObject, string name = null, bool allowNull = false)
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
                            : new ValidationResult(name + " is null!", new[] {name})
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
