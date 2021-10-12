using System.Collections.Generic;

namespace Silky.Http.Core.Configuration
{
    public class GatewayOptions
    {
        internal static string Gateway = "Gateway";

        public GatewayOptions()
        {
            IgnoreWrapperPathPatterns = new List<string>()
            {
                "\\/*.(js|css|html)",
                "\\/(healthchecks|healthz)"
            };
        }

        public string ResponseContentType { get; set; }

        public string JwtSecret { get; set; }
        public ICollection<string> IgnoreWrapperPathPatterns { get; set; }
    }
}