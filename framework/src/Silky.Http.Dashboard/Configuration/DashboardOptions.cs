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
            DisplayWebApiInSwagger = false;
            UseAuth = true;
            UseTenant = true;
            DashboardLoginApi = "/api/account/login";
            WrapperResponse = true;
            TenantNameFiledName = "TenantName";
            UserNameFiledName = "UserName";
            PasswordFiledName = "Password";
        }

        public int StatsPollingInterval { get; set; }

        public bool UseAuth { get; set; }
        
        public bool UseTenant { get; set; }

        public string TenantNameFiledName { get; set; }
        
        public string UserNameFiledName { get; set; }
        
        public string PasswordFiledName { get; set; }

        public string DashboardLoginApi { get; set; }

        public bool DisplayWebApiInSwagger { get; set; }

        public string PathBase { get; set; }

        public bool WrapperResponse { get; set; }

        public ICollection<ExternalLinkOptions> ExternalLinks { get; set; }
    }
}