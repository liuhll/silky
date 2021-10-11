namespace Silky.Http.Core.Diagnostics
{
    public class HttpDiagnosticListenerNames
    {
        private const string RpcPrefix = "Silky.Http.";

        public const string DiagnosticHttpServerListenerName = "Microsoft.AspNetCore";

        public const string BeginHttpHandle = RpcPrefix + "WriteBeginHttpHandle";
        public const string EndHttpHandle = RpcPrefix + "WriteEndHttpHandle";
        public const string ErrorHttpHandle = RpcPrefix + "WriteErrorHttpHandle";
    }
}