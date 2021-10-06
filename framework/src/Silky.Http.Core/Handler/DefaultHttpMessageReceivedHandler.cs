using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core.Extensions;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Executor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal class DefaultHttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        private readonly ISerializer _serializer;

        private GatewayOptions _gatewayOptions;

        public ILogger<DefaultHttpMessageReceivedHandler> Logger { get; set; }

        public DefaultHttpMessageReceivedHandler(
            IOptionsMonitor<RpcOptions> rpcOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IHttpExecutor executor,
            ISerializer serializer,
            IParameterParser parameterParser,
            IServerHandleSupervisor serverHandleSupervisor)
            : base(rpcOptions, executor, parameterParser, serverHandleSupervisor)
        {
            _serializer = serializer;
            _gatewayOptions = gatewayOptions.CurrentValue;
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
            Logger = NullLogger<DefaultHttpMessageReceivedHandler>.Instance;
        }


        protected override async Task HandleResult(HttpContext httpContext, object result)
        {
            httpContext.Response.ContentType = httpContext.GetResponseContentType(_gatewayOptions);
            httpContext.Response.StatusCode = ResponseStatusCode.Success;
            httpContext.Response.SetResultCode(StatusCode.Success);
            if (_gatewayOptions.WrapResult)
            {
                var responseResult = new ResponseResultDto()
                {
                    Data = result,
                    Status = StatusCode.Success,
                };
                var responseData = _serializer.Serialize(responseResult);
                httpContext.Response.ContentLength = responseData.GetBytes().Length;
                await httpContext.Response.WriteAsync(responseData);
            }
            else
            {
                if (result != null)
                {
                    var responseData = _serializer.Serialize(result);
                    httpContext.Response.ContentLength = responseData.GetBytes().Length;
                    await httpContext.Response.WriteAsync(responseData);
                }
                else
                {
                    httpContext.Response.ContentLength = 0;
                    await httpContext.Response.WriteAsync(string.Empty);
                }
            }
        }


        protected override async Task HandleException(Exception exception)
        {
            Logger.LogException(exception);
        }
    }
}