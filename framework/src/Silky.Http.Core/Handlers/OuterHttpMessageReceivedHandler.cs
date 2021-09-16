using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;


namespace Silky.Http.Core.Handlers
{
    internal class OuterHttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        private readonly HttpContext _httpContext;
        private readonly IParameterParser _parameterParser;
        private readonly ISerializer _serializer;
        private GatewayOptions _gatewayOptions;

        public ILogger<OuterHttpMessageReceivedHandler> Logger { get; set; }

        public OuterHttpMessageReceivedHandler(
            IOptionsMonitor<RpcOptions> rpcOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IExecutor executor,
            IHttpContextAccessor httpContextAccessor,
            ISerializer serializer)
            : base(rpcOptions, executor)
        {
            _serializer = serializer;
            _httpContext = httpContextAccessor.HttpContext;
            Check.NotNull(_httpContext, nameof(_httpContext));
            _parameterParser = EngineContext.Current.ResolveNamed<IParameterParser>(HttpMessageType.Outer.ToString());
            _gatewayOptions = gatewayOptions.CurrentValue;
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
            Logger = NullLogger<OuterHttpMessageReceivedHandler>.Instance;
        }

        public override Task Handle(ServiceEntry serviceEntry)
        {
            RpcContext.Context
                .SetAttachment(AttachmentKeys.IsGateway, true);
            return base.Handle(serviceEntry);
        }

        protected override async Task HandleResult(object result)
        {
            _httpContext.Response.ContentType = _httpContext.GetResponseContentType(_gatewayOptions);
            _httpContext.Response.StatusCode = ResponseStatusCode.Success;
            _httpContext.Response.SetResultCode(StatusCode.Success);
            if (_gatewayOptions.WrapResult)
            {
                var responseResult = new ResponseResultDto()
                {
                    Data = result,
                    Status = StatusCode.Success,
                };
                var responseData = _serializer.Serialize(responseResult);
                _httpContext.Response.ContentLength = responseData.GetBytes().Length;
                await _httpContext.Response.WriteAsync(responseData);
            }
            else
            {
                if (result != null)
                {
                    var responseData = _serializer.Serialize(result);
                    _httpContext.Response.ContentLength = responseData.GetBytes().Length;
                    await _httpContext.Response.WriteAsync(responseData);
                }
                else
                {
                    _httpContext.Response.ContentLength = 0;
                    await _httpContext.Response.WriteAsync(string.Empty);
                }
            }
        }


        protected override async Task HandleException(Exception exception)
        {
            Logger.LogException(exception);
        }

        protected override Task<string> ResolveServiceKey()
        {
            return Task.Factory.StartNew(() =>
            {
                string serviceKey = null;
                if (_httpContext.Request.Headers.ContainsKey("serviceKey"))
                {
                    serviceKey = _httpContext.Request.Headers["serviceKey"].ToString();
                }

                return serviceKey;
            });
        }

        protected override Task<object[]> ResolveParameters(ServiceEntry serviceEntry)
        {
            Check.NotNull(_httpContext, nameof(_httpContext));
            return _parameterParser.Parser(serviceEntry);
        }
    }
}