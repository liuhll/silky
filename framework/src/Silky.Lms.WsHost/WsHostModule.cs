using Silky.Lms.Core.Modularity;
using Silky.Lms.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Silky.Lms.WsHost
{
    [DependsOn(typeof(NormHostModule),
        typeof(WebSocketModule))]
    public class WsHostModule : LmsModule
    {
    }
}