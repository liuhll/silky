namespace Silky.Http.Dashboard.Configuration
{
    public class DashboardOptions
    {
        public DashboardOptions()
        {
            PathMatch = "/silky";
            StatsPollingInterval = 2000;
        }
        public string PathMatch { get; set; }
        
        public int StatsPollingInterval { get; set; }
        
        public bool UseAuth { get; set; }
        
        public string PathBase { get; set; }
    }
}