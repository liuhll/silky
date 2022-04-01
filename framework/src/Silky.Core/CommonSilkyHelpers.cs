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
        
    }
}