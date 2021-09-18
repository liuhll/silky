namespace Silky.Core.MiniProfiler
{
    public static class MiniProfilerPrinter
    {
        private static IMiniProfiler _profiler;

        static MiniProfilerPrinter()
        {
            _profiler = EngineContext.Current.Resolve<IMiniProfiler>();
        }

        public static void Print(string category, string state, string message = null, bool isError = false)
        {
            _profiler?.Print(category, state, message, isError);
        }
    }
}