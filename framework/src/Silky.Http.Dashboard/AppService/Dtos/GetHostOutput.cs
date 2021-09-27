namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetHostOutput
    {
        public string Host { get; set; }

        public int InstanceCount { get; set; }

        public int AppServiceCount { get; set; }

        public int LocalServiceEntriesCount { get; set; }
        public bool HasWsService { get; set; }
    }
}