using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Runtime;
using Microsoft.Extensions.Options;
using Silky.Lms.Core;
using Silky.Lms.HttpServer.Configuration;

namespace Silky.Lms.HttpServer.Handlers
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