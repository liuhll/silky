using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.Extensions;
using Silky.Core.Logging;

namespace Silky.Core.Exceptions
{
    public static class ExceptionExtensions
    {
        private static AppSettingsOptions _appSettingsOptions;

        static ExceptionExtensions()
        {
            _appSettingsOptions = EngineContext.Current.GetOptionsMonitor<AppSettingsOptions>((options, name) =>
            {
                _appSettingsOptions = options;
            });
        }

        public static string GetExceptionMessage(this Exception exception)
        {
            var message = exception.IsTimeoutException() ? "Request execution timed out" : exception.Message;
            if (!exception.IsFriendlyException() && _appSettingsOptions.DisplayFullErrorStack)
            {
                message += Environment.NewLine + " Stack information:" + Environment.NewLine + exception.StackTrace;
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

        public static bool IsFriendlyException(this Exception exception)
        {
            return exception.GetExceptionStatusCode().IsFriendlyStatus();
        }
        

        public static bool IsUserFriendlyException(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode.IsUserFriendlyStatus();
        }

        public static bool IsUnauthorized(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode.IsUnauthorized();
        }

        public static bool IsTimeoutException(this Exception exception)
        {
            if (exception is TimeoutException ||
                exception.GetType().FullName == "Polly.Timeout.TimeoutRejectedException")
            {
                return true;
            }

            return false;
        }

        public static bool IsPollyException(this Exception exception)
        {
            if (exception is TimeoutException || exception.GetType().FullName.Contains("Polly"))
            {
                return true;
            }

            return false;
        }

        public static StatusCode GetExceptionStatusCode(this Exception exception)
        {
            var statusCode = StatusCode.NonSilkyException;

            if (exception is IHasErrorCode errorCode)
            {
                statusCode = errorCode.StatusCode;
                return statusCode;
            }

            if (exception.IsTimeoutException())
            {
                statusCode = StatusCode.Timeout;
                return statusCode;
            }

            if (exception.IsPollyException())
            {
                statusCode = StatusCode.ServerError;
                return statusCode;
            }

            if (exception.InnerException != null)
            {
                return exception.InnerException.GetExceptionStatusCode();
            }

            return statusCode;
        }

        public static int GetExceptionStatus(this Exception exception)
        {
            if (exception is IHasErrorCode errorCode)
            {
                return errorCode.Status;
            }

            return (int)exception.GetExceptionStatusCode();
        }

        public static LogLevel GetLogLevel(this Exception exception, LogLevel defaultLevel = LogLevel.Error)
        {
            return (exception as IHasLogLevel)?.LogLevel ?? defaultLevel;
        }
    }
}