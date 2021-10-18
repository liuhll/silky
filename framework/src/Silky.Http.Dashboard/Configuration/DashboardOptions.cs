using System.Collections.Generic;

namespace Silky.Http.Dashboard.Configuration
{
    public class DashboardOptions
    {
        internal static string Dashboard = "Dashboard";

        public DashboardOptions()
        {
            StatsPollingInterval = 2000;
            ExternalLinks = new List<ExternalLinkOptions>();
        }

        public int StatsPollingInterval { get; set; }

        public bool UseAuth { get; set; }

        public string DashboardLoginApi { get; set; }

        public bool DisplayWebApiInSwagger { get; set; }

        public string PathBase { get; set; }

        public bool WrapperResponse { get; set; }

        public ICollection<ExternalLinkOptions> ExternalLinks { get; set; }
    }
}