namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetWebSocketServiceOutput
    {
        public string HostName { get; set; }

        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string Address { get; set; }

        public string Path { get; set; }

        public string ProxyAddress { get; set; }
    }
}