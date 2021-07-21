using SkyApm.Common;

namespace Silky.Rpc.SkyApm.Diagnostics
{
    public class SilkyTags
    {
        public static readonly string RPC_SERVICEID = "Rpc.ServiceId";
        public static readonly string RPC_PARAMETERS = "Rpc.Parameters";
        public static readonly string RPC_LOCAL_ADDRESS = "Rpc.LocalAddress";
        
        public static readonly string RPC_ATTACHMENTS = "Rpc.Attchments";

        public static readonly string ELAPSED_TIME = "ElapsedTime";
        
        public static readonly string RPC_STATUSCODE = "Rpc.StatusCode";
        
        public static readonly string RPC_RESULT = "Rpc.Result";
        
        public static readonly string ISGATEWAY = "IsGateway";
    }
}