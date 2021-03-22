using System.Threading.Tasks;
using Lms.Core;
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
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IParameterParser _parameterParser;
        protected readonly ISerializer _serializer;
        protected readonly RpcOptions _rpcOptions;
        protected readonly IServiceExecutor _serviceExecutor;

        protected MessageReceivedHandlerBase(
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

        public virtual async Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(serviceEntry, nameof(serviceEntry));
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
            var excuteResult = await _serviceExecutor.Execute(serviceEntry, rpcParameters, serviceKey);
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