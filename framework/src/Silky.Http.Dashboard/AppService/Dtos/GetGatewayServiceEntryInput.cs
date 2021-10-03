namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetGatewayServiceEntryInput : PagedRequestDto
    {
        public string ServiceId { get; set; }

        public string SearchKey { get; set; }
    }
}