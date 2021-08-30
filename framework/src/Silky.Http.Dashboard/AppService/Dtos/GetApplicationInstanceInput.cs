using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetApplicationInstanceInput : PagedRequestDto
    {
        
        public ServiceProtocol ServiceProtocol { get; set; } = ServiceProtocol.Tcp;
    }
}