using System;
using System.Collections.Generic;
using System.Linq;
using Lms.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Lms.Core.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionMessage(this Exception exception)
        {
            var message = exception.Message;
            if (!exception.IsBusinessException())
            {
                message += Environment.NewLine + " 堆栈信息:" + Environment.NewLine + exception.StackTrace;
                if (exception.InnerException != null)
                {
                    message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
                }
            }
            return message;
        }

        public static IEnumerable<ValidError> GetValidateErrors(this Exception exception)
        {
            var validateErrors = new List<ValidError>();
            if (exception is IHasValidationErrors)
            {
                foreach (var validationError in ((IHasValidationErrors)exception).ValidationErrors)
                {
                    validateErrors.Add(new ValidError()
                    {
                        ErrorMessage = validationError.ErrorMessage,
                        MemberNames = validationError.MemberNames.ToArray()
                    });
                }
            }

            return validateErrors;
        }

        public static bool IsBusinessException(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode.IsBusinessStatus();

        }
        
        public static bool IsUnauthorized(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode.IsUnauthorized();

        }
        
        public static StatusCode GetExceptionStatusCode(this Exception exception)
        {
            var statusCode = StatusCode.UnPlatformError;
            
            if (exception is LmsException)
            {
                statusCode = ((LmsException)exception).ExceptionCode;
                return statusCode;
            }
            if (exception.InnerException != null)
            {
                return exception.InnerException.GetExceptionStatusCode();
            }
            return statusCode;

        }
        
        public static LogLevel GetLogLevel(this Exception exception, LogLevel defaultLevel = LogLevel.Error)
        {
            return (exception as IHasLogLevel)?.LogLevel ?? defaultLevel;
        }
    }
}