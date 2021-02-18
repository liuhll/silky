using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer
{
    internal class HttpMessageReceivedHandler : IScopedDependency
    {
        private readonly IParameterParser _parameterParser;
        private readonly ISerializer _serializer;
        private readonly RpcOptions _rpcOptions;

        public HttpMessageReceivedHandler(
            IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions)
        {
            _parameterParser = parameterParser;
            _serializer = serializer;
            _rpcOptions = rpcOptions.Value;
        }

        internal async Task Handle(HttpContext context, ServiceEntry serviceEntry)
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
            var excuteResult = await serviceEntry.Executor(serviceKey, rpcParameters);
            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = 200;
            if (excuteResult != null)
            {
                var responseData = _serializer.Serialize(excuteResult);
                context.Response.ContentLength = responseData.GetBytes().Length;
                await context.Response.WriteAsync(responseData);
            }
            else
            {
                context.Response.ContentLength = 0;
                await context.Response.WriteAsync(string.Empty);
            }
        }
    }
}