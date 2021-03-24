using Lms.Core.Modularity;
using Lms.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Lms.WsHost
{
    [DependsOn(typeof(NormHostModule),
        typeof(WebSocketModule))]
    public class WsHostModule : LmsModule
    {
    }
}