using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.MiniProfiler;
using Silky.Http.Swagger.Configuration;
using StackExchange.Profiling;

namespace Silky.Http.MiniProfiler
{
    public class DefaultMiniProfiler : IMiniProfiler
    {

        private static SwaggerDocumentOptions swaggerDocumentOptions = null;

        static DefaultMiniProfiler()
        {
            swaggerDocumentOptions = EngineContext.Current.GetOptionsMonitor<SwaggerDocumentOptions>(
                (options, name) => { swaggerDocumentOptions = options; });
        }

        /// <summary>
        /// Print information to MiniProfiler
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="state">State</param>
        /// <param name="message">Message</param>
        /// <param name="isError">Is it a error message</param>
        public void Print(string category, string state, string message = null, bool isError = false)
        {
            var customTiming = StackExchange.Profiling.MiniProfiler.Current?.CustomTiming(category,
                string.IsNullOrWhiteSpace(message) ? $"{category.ToTitleCase()} {state}" : message, state);
            if (customTiming == null) return;
            // 判断是否是警告消息
            if (isError) customTiming.Errored = true;
        }
    }
}