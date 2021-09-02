namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryInput : PagedRequestDto
    {
        public string Application { get; set; }

        public string AppService { get; set; }

        public string Name { get; set; }

        public bool? IsOnline { get; set; }
    }
}