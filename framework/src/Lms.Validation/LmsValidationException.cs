using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Lms.Core.Exceptions;
using Lms.Core.Extensions.Collections.Generic;
using Lms.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Lms.Validation
{
    public class LmsValidationException : LmsException, IHasValidationErrors, IExceptionWithSelfLogging
    {
        public LmsValidationException(string message) : base(message, StatusCode.ValidateError)
        {
            ValidationErrors = new List<ValidationResult>();
            LogLevel = LogLevel.Warning;
        }

        public LmsValidationException(string message, IList<ValidationResult> validationErrors) : this(message)
        {
            ValidationErrors = validationErrors;
            LogLevel = LogLevel.Warning;
        }


        public IList<ValidationResult> ValidationErrors { get; }

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