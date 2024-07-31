using Microsoft.AspNetCore.Http;
using Silky.Core.Extensions;
using StackExchange.Profiling;

namespace Silky.Core.MiniProfiler
{
    public static class MiniProfilerHelper
    {
        /// <summary>
        /// Print information to MiniProfiler
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="state">State</param>
        /// <param name="message">Message</param>
        /// <param name="isError">Is it a error message</param>
        public static void Print(string category, string state, string message = null, bool isError = false)
        {
            if (!CanBeMiniProfiler()) return;

            var customTiming = StackExchange.Profiling.MiniProfiler.Current?.CustomTiming(category,
                string.IsNullOrWhiteSpace(message) ? $"{category.ToTitleCase()} {state}" : message, state);
            if (customTiming == null) return;

            // 判断是否是警告消息
            if (isError) customTiming.Errored = true;
        }

        private static bool CanBeMiniProfiler()
        {
            var httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext == null) return false;
            if (!(httpContext.Request.Headers.TryGetValue("request-from", out var value) && value == "swagger"))
                return false;

            return true;
        }
    }
}