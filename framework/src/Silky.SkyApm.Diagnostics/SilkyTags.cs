using SkyApm.Common;

namespace Silky.Rpc.SkyApm.Diagnostics
{
    public class SilkyTags
    {
        public static readonly string RPC_SERVICEENTRYID = "Rpc.ServiceEntryId";
        public static readonly string RPC_PARAMETERS = "Rpc.Parameters";
        public static readonly string RPC_CLIENT_ADDRESS = "Rpc.ClientAddress";
        public static readonly string RPC_SERVER_ADDRESS = "Rpc.ServerAddress";
        
        public static readonly string RPC_ATTACHMENTS = "Rpc.Attchments";

        public static readonly string ELAPSED_TIME = "ElapsedTime";
        
        public static readonly string RPC_STATUSCODE = "Rpc.StatusCode";
        
        public static readonly string RPC_RESULT = "Rpc.Result";
        
        public static readonly string RPC_SERVIC_METHODENAME = "Rpc.ServiceMethodName";
        
        public static readonly string ISGATEWAY = "IsGateway";
    }
}