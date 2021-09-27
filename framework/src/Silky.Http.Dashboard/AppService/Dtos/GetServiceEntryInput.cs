namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryInput : PagedRequestDto
    {
        public string HostName { get; set; }

        public string ServiceId { get; set; }
        
        public string ServiceEntryId { get; set; }

        public string Name { get; set; }

        public bool? IsEnable { get; set; }
    }
}