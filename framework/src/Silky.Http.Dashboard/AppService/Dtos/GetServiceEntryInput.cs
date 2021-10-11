namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryInput : PagedRequestDto
    {
        public string HostName { get; set; }

        public string ServiceId { get; set; }

        public string ServiceEntryId { get; set; }

        public string SearchKey { get; set; }

        public bool? IsEnable { get; set; }

        public bool? IsAllowAnonymous { get; set; }
        public bool? ProhibitExtranet { get; set; }
        public bool? IsDistributeTransaction { get; set; }
        public bool? MultipleServiceKey { get; set; }

        public bool? IsSystem { get; set; }
    }
}