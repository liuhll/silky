namespace Silky.SkyApm.Diagnostics.Abstraction
{
    public class SilkyTags
    {
        public static readonly string SERVICEENTRYID = "ServiceEntryId";
        public static readonly string RPC_CLIENT_ENDPOINT = "Rpc.ClientAddress";
        public static readonly string RPC_LOCAL_RPCENDPOINT = "Rpc.LocalAddress";
        public static readonly string RPC_REMOTE_PORT = "Rpc.RequestPort";

        public static readonly string ELAPSED_TIME = "ElapsedTime";

        public static readonly string RPC_STATUSCODE = "Rpc.StatusCode";

        public static readonly string ISGATEWAY = "IsGateway";
        public static readonly string SERVICEKEY = "ServiceKey";

        public static readonly string IS_LOCAL_SERVICEENTRY = "IsLocalServiceEntry";
        public static readonly string WEBAPI = "Webapi";
        public static readonly string HTTPMETHOD = "HttpMethod";
        public static readonly string RPC_SHUNTSTRATEGY = "ShuntStrategy";

        public static readonly string RPC_SELECTEDADDRESS = "SelectedAddress";
        public static readonly string FALLBACK_EXEC_TYPE = "FallbackExecType";
        public static readonly string FALLBACK_TYPE = "FallbackType";
        public static readonly string FALLBACK_METHOD = "FallbackMethod";
    }
}