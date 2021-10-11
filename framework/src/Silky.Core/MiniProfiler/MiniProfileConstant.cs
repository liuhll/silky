namespace Silky.Core.MiniProfiler
{
    public static class MiniProfileConstant
    {
        public const string MiniProfilerRouteBasePath = "/index-mini-profiler";

        public static class Route
        {
            public const string Name = "Route";

            public static class State
            {
                public const string FindServiceEntry = "FindServiceEntry";
                public const string FindServiceKey = "FindServiceKey";
            }
        }

        public static class RemoteInvoker
        {
            public const string Name = "RemoteInvoker";

            public static class State
            {
                public const string Prepare = "Prepare";
                public const string Success = "Success";
                public const string Fail = "Fail";
                public static string End = "End";
            }
        }

        public static class Rpc
        {
            public const string Name = "Rpc";

            public static class State
            {
                public const string HashKey = "HashKey";

                public const string Start = "Start";

                public const string FindServiceRoute = "FindServiceRoute";

                public const string SelectedAddress = "SelectedServerEndpoint";

                public const string Success = "Success";

                public const string Fail = "Fail";

                public const string MarkAddressFail = "MarkAddressFail";
            }
        }

        public static class Caching
        {
            public const string Name = "Caching";

            public static class State
            {
                public static string CacheEnabled = "CacheEnabled";
                public static string NotSet = "NotSet";
                public static string RemoveCaching = "RemoveCaching";
                public static string GetCaching = "GetCaching";
                public static string UpdateCaching = "UpdateCaching";
                public static string GetCachingSuccess = "GetCachingSuccess";
                public static string GetCachingFail = "GetCachingFail";
            }
        }

        public static class ValidationInterceptor
        {
            public const string Name = "ValidationInterceptor";

            public static class State
            {
                public static string Begin = "Begin";
                public const string Success = "Success";

                public const string Fail = "Fail";
            }
        }

        public static class FallBackExecutor
        {
            public const string Name = "FallBackExecutor";

            public static class State
            {
                public static string Begin = "Begin";
                public const string Success = "Success";

                public const string Fail = "Fail";
            }
        }
    }
}