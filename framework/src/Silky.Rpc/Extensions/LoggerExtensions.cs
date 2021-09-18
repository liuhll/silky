using Microsoft.Extensions.Logging;
using Silky.Core.Logging;
using Silky.Rpc.MiniProfiler;

namespace Silky.Rpc.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogWithMiniProfiler(this ILogger logger, string category, string state,
            string message, bool isError = false, LogLevel? level = null)
        {
            level ??= isError ? LogLevel.Error : LogLevel.Debug;
            logger.LogWithLevel(level.Value, message);
            MiniProfilerPrinter.Print(category, state, message, isError);
        }
    }
}