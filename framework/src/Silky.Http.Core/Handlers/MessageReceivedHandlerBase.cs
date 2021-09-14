using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Core.Rpc;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Messages;
using Silky.Rpc.MiniProfiler;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IParameterParser _parameterParser;
        protected readonly ISerializer _serializer;
        protected readonly IExecutor _executor;

        protected GatewayOptions _gatewayOptions;
        protected RpcOptions _rpcOptions;

        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);

        protected MessageReceivedHandlerBase(
            IParameterParser parameterParser,
            ISerializer serializer,
            IOptionsMonitor<RpcOptions> rpcOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IExecutor executor)
        {
            _parameterParser = parameterParser;
            _serializer = serializer;
            _executor = executor;
            _rpcOptions = rpcOptions.CurrentValue;
            _gatewayOptions = gatewayOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
        }

        public virtual async Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var sp = Stopwatch.StartNew();
            var requestParameters = await _parameterParser.Parser(context.Request, serviceEntry);
            RpcContext.Context
                .SetAttachment(AttachmentKeys.RequestHeader, requestParameters[ParameterFrom.Header]);
            RpcContext.Context
                .SetAttachment(AttachmentKeys.IsGatewayHost, true);
            var rpcParameters = serviceEntry.ResolveParameters(requestParameters);
            string serviceKey = null;

            if (context.Request.Headers.ContainsKey("serviceKey"))
            {
                serviceKey = context.Request.Headers["serviceKey"].ToString();
                RpcContext.Context.SetAttachment(AttachmentKeys.ServiceKey, serviceKey);
                MiniProfilerPrinter.Print(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    $"serviceKey => {serviceKey}");
            }
            else
            {
                MiniProfilerPrinter.Print(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    "No serviceKey is set");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.RpcToken, _rpcOptions.Token);

            var tracingTimestamp = TracingBefore(new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceId,
                ServiceEntryId = serviceEntry.Id,
                Attachments = RpcContext.Context.GetContextAttachments(),
                Parameters = rpcParameters
            }, context.TraceIdentifier, serviceEntry);
            object executeResult = null;
            try
            {
                executeResult = await _executor.Execute(serviceEntry, rpcParameters, serviceKey);
                TracingAfter(tracingTimestamp, context.TraceIdentifier, serviceEntry, new RemoteResultMessage()
                {
                    ServiceEntryId = serviceEntry.Id,
                    StatusCode = StatusCode.Success,
                    Result = executeResult,
                });
            }
            catch (Exception e)
            {
                TracingError(tracingTimestamp, context.TraceIdentifier, serviceEntry, e.GetExceptionStatusCode(), e);
                throw;
            }
            finally
            {
                sp.Stop();
            }

            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = ResponseStatusCode.Success;
            context.Response.SetResultCode(StatusCode.Success);
            if (_gatewayOptions.WrapResult)
            {
                var responseResult = new ResponseResultDto()
                {
                    Data = executeResult,
                    Status = StatusCode.Success,
                };
                var responseData = _serializer.Serialize(responseResult);
                context.Response.ContentLength = responseData.GetBytes().Length;
                await context.Response.WriteAsync(responseData);
            }
            else
            {
                if (executeResult != null)
                {
                    var responseData = _serializer.Serialize(executeResult);
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

        private long? TracingBefore(RemoteInvokeMessage message, string messageId, ServiceEntry serviceEntry)
        {
            if (serviceEntry.IsLocal && s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcRequest))
            {
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntryId = message.ServiceEntryId,
                    Message = message
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcRequest, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry,
            RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeResultEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntry.Id,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcRequest, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry,
            StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeExceptionEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntry.Id,
                    StatusCode = statusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ServerAddress).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }
    }
}