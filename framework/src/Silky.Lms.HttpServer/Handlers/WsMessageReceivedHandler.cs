using System.Threading.Tasks;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Runtime;
using Silky.Lms.Rpc.Runtime.Client;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Lms.HttpServer.Configuration;
using Silky.Lms.Rpc;

namespace Silky.Lms.HttpServer.Handlers
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
            throw new LmsException("websocket通信请依赖Lms.WebSocketServer包");
        }
    }
}