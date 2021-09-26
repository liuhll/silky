using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silky.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.Logging;
using Silky.Core.Exceptions;
using Silky.Core.MiniProfiler;

namespace Silky.Core.Logging
{
    public static class LoggerExtensions
    {
        public static void LogWithLevel(this ILogger logger, LogLevel logLevel, string message, params object[] agrs)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.LogCritical(message, agrs);
                    break;
                case LogLevel.Error:
                    logger.LogError(message, agrs);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(message, agrs);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(message, agrs);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(message, agrs);
                    break;
                default: // LogLevel.Debug || LogLevel.None
                    logger.LogDebug(message, agrs);
                    break;
            }
        }

        public static void LogWithLevel(this ILogger logger, LogLevel logLevel, string message, Exception exception)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.LogCritical(exception, message);
                    break;
                case LogLevel.Error:
                    logger.LogError(exception, message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(exception, message);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(exception, message);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(exception, message);
                    break;
                default: // LogLevel.Debug || LogLevel.None
                    logger.LogDebug(exception, message);
                    break;
            }
        }

        public static void LogException(this ILogger logger, Exception ex, LogLevel? level = null)
        {
            var selectedLevel = level ?? ex.GetLogLevel();

            logger.LogWithLevel(selectedLevel, ex.Message, ex);
            LogKnownProperties(logger, ex, selectedLevel);
            LogSelfLogging(logger, ex);
            LogData(logger, ex, selectedLevel);
        }

        public static void LogWithMiniProfiler(this ILogger logger, string category, string state,
            string message, bool isError = false, LogLevel? level = null, params object[] args)
        {
            level ??= isError ? LogLevel.Error : LogLevel.Debug;
            logger.LogWithLevel(level.Value, message, args);
            if (args != null && args.Any())
            {
                MiniProfilerPrinter.Print(category, state, string.Format(message, args), isError);
            }
            else
            {
                MiniProfilerPrinter.Print(category, state, message, isError);
            }
        }

        private static void LogKnownProperties(ILogger logger, Exception exception, LogLevel logLevel)
        {
            if (exception is IHasErrorCode exceptionWithErrorCode)
            {
                logger.LogWithLevel(logLevel, "Code:" + exceptionWithErrorCode.StatusCode);
            }

            if (exception is IHasErrorDetails exceptionWithErrorDetails)
            {
                logger.LogWithLevel(logLevel, "Details:" + exceptionWithErrorDetails.Details);
            }
        }

        private static void LogData(ILogger logger, Exception exception, LogLevel logLevel)
        {
            if (exception.Data == null || exception.Data.Count <= 0)
            {
                return;
            }

            var exceptionData = new StringBuilder();
            exceptionData.AppendLine("---------- Exception Data ----------");
            foreach (var key in exception.Data.Keys)
            {
                exceptionData.AppendLine($"{key} = {exception.Data[key]}");
            }

            logger.LogWithLevel(logLevel, exceptionData.ToString());
        }

        private static void LogSelfLogging(ILogger logger, Exception exception)
        {
            var loggingExceptions = new List<IExceptionWithSelfLogging>();

            if (exception is IExceptionWithSelfLogging)
            {
                loggingExceptions.Add(exception as IExceptionWithSelfLogging);
            }
            else if (exception is AggregateException && exception.InnerException != null)
            {
                var aggException = exception as AggregateException;
                if (aggException.InnerException is IExceptionWithSelfLogging)
                {
                    loggingExceptions.Add(aggException.InnerException as IExceptionWithSelfLogging);
                }

                foreach (var innerException in aggException.InnerExceptions)
                {
                    if (innerException is IExceptionWithSelfLogging)
                    {
                        loggingExceptions.AddIfNotContains(innerException as IExceptionWithSelfLogging);
                    }
                }
            }

            foreach (var ex in loggingExceptions)
            {
                ex.Log(logger);
            }
        }
    }
}