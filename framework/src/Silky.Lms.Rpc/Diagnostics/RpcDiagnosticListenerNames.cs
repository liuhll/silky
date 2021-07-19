namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcDiagnosticListenerNames
    {
        private const string RpcPrefix = "Silky.Lms.Rpc.";
        
        public const string DiagnosticClientListenerName = "DiagnosticClientListenerName";
        
        public const string DiagnosticServerListenerName = "DiagnosticServerListenerName";

        public const string BeginRpcRequest = RpcPrefix + "WriteBeginRpcRequest";
        public const string EndRpcRequest = RpcPrefix + "WriteEndRpcRequest";
        public const string ErrorRpcRequest = RpcPrefix + "WriteErrorRpcRequest";
        
        public const string BeginRpcServerHandler = RpcPrefix + "WriteBeginRpcServerHandler";
        public const string EndRpcServerHandler = RpcPrefix + "WriteEndRpcServerHandler";
        public const string ErrorRpcServerHandler = RpcPrefix + "WriteErrorRpcServerHandler";
    }
}