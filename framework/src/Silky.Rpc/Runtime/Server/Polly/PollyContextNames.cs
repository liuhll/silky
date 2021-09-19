namespace Silky.Rpc.Runtime.Server
{
    internal static class PollyContextNames
    {
        public const string ServerHandlerOperationKey = "ServerHandler";

        public const string ServiceEntry = "ServiceEntry";

        public const string Exception = "Exception";

        public const string ElapsedTimeMs = "ElapsedTimeMs";

        public const string TracingTimestamp = "TracingTimestamp";
    }
}