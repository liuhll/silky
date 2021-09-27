namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceOutput
    {
        public string HostName { get; set; }

        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public int InstanceCount { get; set; }
    }
}