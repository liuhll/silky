namespace Silky.Rpc.Diagnostics
{
    public class RpcDiagnosticListenerNames
    {
        private const string RpcPrefix = "Silky.Rpc.";
        
        public const string DiagnosticClientListenerName = "DiagnosticClientListener";
        
        public const string DiagnosticServerListenerName = "DiagnosticServerListener";
        
        //public const string DiagnosticExecutorListenerName = "DiagnosticExecutorListener";

        public const string BeginRpcRequest = RpcPrefix + "WriteBeginRpcRequest";
        public const string EndRpcRequest = RpcPrefix + "WriteEndRpcRequest";
        public const string ErrorRpcRequest = RpcPrefix + "WriteErrorRpcRequest";
        
        public const string BeginRpcServerHandler = RpcPrefix + "WriteBeginRpcServerHandler";
        public const string EndRpcServerHandler = RpcPrefix + "WriteEndRpcServerHandler";
        public const string ErrorRpcServerHandler = RpcPrefix + "WriteErrorRpcServerHandler";

        public const string LocalMethodExecute = RpcPrefix + "LocalMethodExecute";
    }
}