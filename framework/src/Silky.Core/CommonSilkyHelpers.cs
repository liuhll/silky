using System;
using System.Threading;
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
        
        public static Timer Create(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            ArgumentNullException.ThrowIfNull(callback);

            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            var restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                return new Timer(callback, state, dueTime, period);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
        
    }
}