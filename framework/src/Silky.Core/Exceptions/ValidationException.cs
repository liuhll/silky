using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Silky.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;

namespace Silky.Core.Exceptions
{
    public class ValidationException : SilkyException, IHasValidationErrors, IExceptionWithSelfLogging
    {
        public ValidationException(string message) : base(message, StatusCode.ValidateError)
        {
            ValidationErrors = Array.Empty<ValidError>();
            LogLevel = LogLevel.Warning;
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException,
            StatusCode.ValidateError)
        {
            ValidationErrors = Array.Empty<ValidError>();
            LogLevel = LogLevel.Warning;
        }

        public ValidationException(string message, IList<ValidationResult> validationErrors) : this(message)
        {
            ValidationErrors = validationErrors.Select(p => new ValidError()
            {
                MemberNames = p.MemberNames.ToArray(),
                ErrorMessage = p.ErrorMessage,
            }).ToArray();
            LogLevel = LogLevel.Warning;
        }

        public ValidationException(string exceptionMessage, ValidError[] validationErrors) : base(exceptionMessage,
            StatusCode.ValidateError)
        {
            ValidationErrors = validationErrors;
            LogLevel = LogLevel.Warning;
        }


        public IList<ValidError> ValidationErrors { get; }

        public LogLevel LogLevel { get; set; }

        public void Log(ILogger logger)
        {
            if (ValidationErrors.IsNullOrEmpty())
            {
                return;
            }

            var validationErrors = new StringBuilder();
            validationErrors.AppendLine("There are " + ValidationErrors.Count + " validation errors:");
            foreach (var validationResult in ValidationErrors)
            {
                var memberNames = "";
                if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
                {
                    memberNames = " (" + string.Join(", ", validationResult.MemberNames) + ")";
                }

                validationErrors.AppendLine(validationResult.ErrorMessage + memberNames);
            }

            logger.LogWithLevel(LogLevel, validationErrors.ToString());
        }
    }
}