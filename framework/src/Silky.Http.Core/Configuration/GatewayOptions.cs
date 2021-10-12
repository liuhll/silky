namespace Silky.Http.Core.Configuration
{
    public class GatewayOptions
    {
        internal static string Gateway = "Gateway";

        public bool WrapResult { get; set; }

        public string ResponseContentType { get; set; }

        public string JwtSecret { get; set; }
        public string IgnoreWrapperPathPattern { get; set; } = "swagger";
    }
}