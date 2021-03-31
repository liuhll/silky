using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Core.Serialization;
using Lms.HttpServer.Configuration;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer.Handlers
{
    internal class WsMessageReceivedHandler : MessageReceivedHandlerBase
    {

        public WsMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IOptions<GatewayOptions> gatewayOptions,
            IServiceExecutor serviceExecutor,
            IRemoteServiceInvoker remoteServiceInvoker,
            IOptions<GovernanceOptions> governanceOptions)
            : base(parameterParser,
                serializer,
                rpcOptions,
                gatewayOptions,
                serviceExecutor)
        {
        }

        public override Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new LmsException("websocket通信请依赖Lms.WebSocketServer包");
        }
    }
}