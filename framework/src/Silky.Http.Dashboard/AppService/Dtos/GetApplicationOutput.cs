namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetApplicationOutput
    {
        public string Application { get; set; }

        public int InstanceCount { get; set; }

        public int AppServiceCount { get; set; }

        public int ServiceEntriesCount { get; set; }
        public bool HasWsService { get; set; }
    }
}