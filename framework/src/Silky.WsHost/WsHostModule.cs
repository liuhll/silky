using Silky.Core.Modularity;
using Silky.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Silky.WsHost
{
    [DependsOn(typeof(NormHostModule),
        typeof(WebSocketModule))]
    public class WsHostModule : StartUpModule
    {
    }
}