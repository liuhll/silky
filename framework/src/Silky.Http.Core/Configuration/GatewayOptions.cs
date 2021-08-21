namespace Silky.Http.Core.Configuration
{
    public class GatewayOptions
    {
        public static string Gateway = "Gateway";

        public bool UseDetailedExceptionPage { get; set; }
        public bool WrapResult { get; set; }
        public bool EnableSwaggerDoc { get; set; }

        public int FuseTimes { get; set; } = 3;
        public string JwtSecret { get; set; }
    }
}