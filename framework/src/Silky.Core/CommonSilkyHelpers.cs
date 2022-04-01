using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Silky.Core
{
    public partial class CommonSilkyHelpers
    {
        public static ISilkyFileProvider DefaultFileProvider { get; set; }
        
        private static readonly int TimerEpsilonMilliseconds = 14;
        
        public static long GetTimerDueTime(TimeSpan timeout, long maxTimerDueTime)
        {
            var dueTimeMilliseconds = timeout.Ticks / TimeSpan.TicksPerMillisecond;
            
            dueTimeMilliseconds += TimerEpsilonMilliseconds;

            dueTimeMilliseconds = Math.Min(dueTimeMilliseconds, maxTimerDueTime);
            // Timer can't have a negative due time
            dueTimeMilliseconds = Math.Max(dueTimeMilliseconds, 0);

            return dueTimeMilliseconds;
        }
        
        public static IHeaderDictionary GetTrailersDestination(HttpResponse response)
        {
            if (response.HasStarted)
            {
                // The response has content so write trailers to a trailing HEADERS frame
                var feature = response.HttpContext.Features.Get<IHttpResponseTrailersFeature>();
                if (feature?.Trailers == null || feature.Trailers.IsReadOnly)
                {
                    throw new InvalidOperationException("Trailers are not supported for this response. The server may not support gRPC.");
                }

                return feature.Trailers;
            }
            else
            {
                // The response is "Trailers-Only". There are no gRPC messages in the response so the status
                // and other trailers can be placed in the header HEADERS frame
                return response.Headers;
            }
        }
    }
}