using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetHostInstanceInput : PagedRequestDto
    {
        public ServiceProtocol? ServiceProtocol { get; set; }
    }
}