namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcDiagnosticListenerNames
    {
        private const string RpcPrefix = "Silky.Lms.Rpc.";
        
        public const string DiagnosticListenerName = "RpcDiagnosticListener";

        public const string BeforeRpcInvoker = RpcPrefix + "WriteRpcInvokerBefore";
        public const string AfterRpcInvoker = RpcPrefix + "WriteRpcInvokerAfter";
        public const string ErrorRpcInvoker = RpcPrefix + "WriteRpcInvokerError";
    }
}