namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class PagedRequestDto
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}