using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Microsoft.Extensions.Options;
using Silky.Http.Core.Configuration;
using Silky.Rpc;
using Silky.Rpc.MiniProfiler;

namespace Silky.Http.Core.Handlers
{
    internal class HttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public HttpMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptionsMonitor<RpcOptions> rpcOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IExecutor executor)
            : base(parameterParser,
                serializer,
                rpcOptions,
                gatewayOptions,
                executor)
        {
        }
    }
}