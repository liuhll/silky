using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Silky.Core
{
    public class EngineContext
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static IEngine Create()
        {
            return Singleton<IEngine>.Instance ?? (Singleton<IEngine>.Instance = new SilkyEngine());
        }

        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }

        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Create();
                }

                return Singleton<IEngine>.Instance;
            }
        }

        static EngineContext()
        {
            UnmanagedObjects = new ConcurrentBag<IDisposable>();
        }

        public static readonly ConcurrentBag<IDisposable> UnmanagedObjects;

        private const int GC_COLLECT_INTERVAL_SECONDS = 5;

        private static DateTime? LastGCCollectTime { get; set; }

        public static void DisposeUnmanagedObjects()
        {
            foreach (var dsp in UnmanagedObjects)
            {
                try
                {
                    dsp?.Dispose();
                }
                finally
                {
                }
            }

            // 强制手动回收 GC 内存
            if (UnmanagedObjects.Any())
            {
                var nowTime = DateTime.UtcNow;
                if ((LastGCCollectTime == null ||
                     (nowTime - LastGCCollectTime.Value).TotalSeconds > GC_COLLECT_INTERVAL_SECONDS))
                {
                    LastGCCollectTime = nowTime;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            UnmanagedObjects.Clear();
        }
    }
}