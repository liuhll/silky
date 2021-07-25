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

namespace Silky.Http.Core.Handlers
{
    internal class WsMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public WsMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IOptions<GatewayOptions> gatewayOptions,
            IServiceExecutor serviceExecutor,
            IRemoteServiceInvoker remoteServiceInvoker,
            IOptions<GovernanceOptions> governanceOptions,
            IMiniProfiler miniProfiler)
            : base(parameterParser,
                serializer,
                rpcOptions,
                gatewayOptions,
                serviceExecutor,
                miniProfiler)
        {
        }

        public override Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new SilkyException("websocket通信请依赖Silky.WebSocketServer包");
        }
    }
}