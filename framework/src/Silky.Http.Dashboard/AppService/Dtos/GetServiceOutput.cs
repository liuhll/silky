namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceOutput
    {
        public string Application { get; set; }

        public string AppService { get; set; }

        public int InstanceCount { get; set; }
    }
}