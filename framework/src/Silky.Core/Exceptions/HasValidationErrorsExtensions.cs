using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Exceptions
{
    public static class HasValidationErrorsExtensions
    {
        public static TException WithValidationError<TException>([NotNull] this TException exception,
            [NotNull] ValidationResult validationError)
            where TException : IHasValidationErrors
        {
            Check.NotNull(exception, nameof(exception));
            Check.NotNull(validationError, nameof(validationError));

            exception.ValidationErrors.Add(new ValidError()
            {
                ErrorMessage = validationError.ErrorMessage,
                MemberNames = validationError.MemberNames.ToArray(),
            });

            return exception;
        }

        public static TException WithValidationError<TException>([NotNull] this TException exception,
            string errorMessage, params string[] memberNames)
            where TException : IHasValidationErrors
        {
            var validationResult = memberNames.IsNullOrEmpty()
                ? new ValidationResult(errorMessage)
                : new ValidationResult(errorMessage, memberNames);

            return exception.WithValidationError(validationResult);
        }

        public static IEnumerable<ValidError> GetValidateErrors<TException>(this TException exception)
            where TException : IHasValidationErrors
        {
            var validateErrors = new List<ValidError>();

            foreach (var validateError in exception.ValidationErrors)
            {
                validateErrors.Add(new ValidError()
                {
                    ErrorMessage = validateError.ErrorMessage,
                    MemberNames = validateError.MemberNames.ToArray()
                });
            }

            return validateErrors;
        }
    }
}