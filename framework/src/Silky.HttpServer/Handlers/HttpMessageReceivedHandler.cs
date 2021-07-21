using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Microsoft.Extensions.Options;
using Silky.HttpServer.Configuration;
using Silky.Rpc;

namespace Silky.HttpServer.Handlers
{
    internal class HttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public HttpMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IOptions<GatewayOptions> gatewayOptions,
            IServiceExecutor serviceExecutor,
            IMiniProfiler miniProfiler)
            : base(parameterParser,
                serializer,
                rpcOptions,
                gatewayOptions,
                serviceExecutor,
                miniProfiler)
        {
        }
    }
}