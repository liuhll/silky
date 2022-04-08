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
            GlobalAuthorize = false;
            TimeoutMillSeconds = 0;
        }

        public string ResponseContentType { get; set; }

        public int TimeoutMillSeconds { get; set; }

        public string JwtSecret { get; set; }

        public bool GlobalAuthorize { get; set; }
        public ICollection<string> IgnoreWrapperPathPatterns { get; set; }
    }
}