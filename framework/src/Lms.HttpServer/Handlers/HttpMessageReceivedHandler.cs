using Lms.Core.Serialization;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer.Handlers
{
    internal class HttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public HttpMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IServiceExecutor serviceExecutor)
            : base(parameterParser,
                serializer,
                rpcOptions,
                serviceExecutor)
        {
        }
    }
}