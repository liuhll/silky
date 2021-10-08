namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServerHandlePagedRequestDto : PagedRequestDto
    {
        public string ServiceEntryId { get; set; }

        public string SearchKey { get; set; }
    }
}