namespace Lms.HttpServer.Configuration
{
    public class GatewayOptions
    {
        public static string Gateway = "Gateway";
        public bool DisplayFullErrorStack { get; set; } = false;

        public bool WrapResult { get; set; } = false;
    }
}