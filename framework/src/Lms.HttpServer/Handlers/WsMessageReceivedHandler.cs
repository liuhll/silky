using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer.Handlers
{
    internal class WsMessageReceivedHandler : IMessageReceivedHandler
    {
        private readonly IParameterParser _parameterParser;
        private readonly ISerializer _serializer;
        private readonly RpcOptions _rpcOptions;
        private readonly IServiceExecutor _serviceExecutor;

        public WsMessageReceivedHandler(
            IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions, 
            IServiceExecutor serviceExecutor)
        {
            _parameterParser = parameterParser;
            _serializer = serializer;
            _serviceExecutor = serviceExecutor;
            _rpcOptions = rpcOptions.Value;
        }
        public async Task Handle(HttpContext context, [NotNull]ServiceEntry serviceEntry)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var requestParameters = await _parameterParser.Parser(context.Request, serviceEntry);
                RpcContext.GetContext().SetAttachment("requestHeader", requestParameters[ParameterFrom.Header]);
                var rpcParameters = serviceEntry.ResolveParameters(requestParameters);
                string serviceKey = null;

                if (context.Request.Headers.ContainsKey("serviceKey"))
                {
                    serviceKey = context.Request.Headers["serviceKey"].ToString();
                    RpcContext.GetContext().SetAttachment("serviceKey", serviceKey);
                }

                RpcContext.GetContext().SetAttachment("token", _rpcOptions.Token);
                await _serviceExecutor.Execute(serviceEntry, rpcParameters, serviceKey);
              
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}