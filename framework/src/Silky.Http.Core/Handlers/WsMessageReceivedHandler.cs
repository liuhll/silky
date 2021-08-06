using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Http.Core.Configuration;
using Silky.Rpc;
using Silky.Rpc.MiniProfiler;

namespace Silky.Http.Core.Handlers
{
    internal class WsMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public WsMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptionsMonitor<RpcOptions> rpcOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IServiceExecutor serviceExecutor)
            : base(parameterParser,
                serializer,
                rpcOptions,
                gatewayOptions,
                serviceExecutor)
        {
        }

        public override Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new SilkyException("Please rely on Silky.WebSocketServer package for websocket communication");
        }
    }
}