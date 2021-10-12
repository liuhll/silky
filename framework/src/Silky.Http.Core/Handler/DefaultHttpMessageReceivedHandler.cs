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
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.Http.Core.Handlers
{
    internal class DefaultHttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        private readonly ISerializer _serializer;

        private GatewayOptions _gatewayOptions;

        public ILogger<DefaultHttpMessageReceivedHandler> Logger { get; set; }

        public DefaultHttpMessageReceivedHandler(
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IHttpExecutor executor,
            ISerializer serializer,
            IParameterParser parameterParser,
            ICurrentRpcToken currentRpcToken,
            IHttpHandleDiagnosticListener httpHandleDiagnosticListener)
            : base(executor, parameterParser, currentRpcToken,
                httpHandleDiagnosticListener)
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
            result ??= string.Empty;

            var responseData = _serializer.Serialize(result);
            await httpContext.Response.WriteAsync(responseData);
        }


        protected override async Task HandleException(Exception exception)
        {
            Logger.LogException(exception);
        }
    }
}