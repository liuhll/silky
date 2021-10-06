using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Rpc;
using Silky.Http.Core.Executor;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Transport.Messages;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IHttpExecutor _executor;
        private readonly IParameterParser _parameterParser;

        protected RpcOptions _rpcOptions;
        public ILogger<MessageReceivedHandlerBase> Logger { get; set; }


        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);

        protected MessageReceivedHandlerBase(
            IOptionsMonitor<RpcOptions> rpcOptions,
            IHttpExecutor executor,
            IParameterParser parameterParser)
        {
            _executor = executor;
            _parameterParser = parameterParser;
            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);
            Logger = NullLogger<MessageReceivedHandlerBase>.Instance;
        }

        public virtual async Task Handle([NotNull] ServiceEntry serviceEntry, [NotNull] HttpContext httpContext)
        {
            Check.NotNull(serviceEntry, nameof(httpContext));
            Check.NotNull(httpContext, nameof(httpContext));
            var path = httpContext.Request.Path;
            var method = httpContext.Request.Method.ToEnum<HttpMethod>();
            Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                MiniProfileConstant.Route.State.FindServiceEntry,
                $"Find the ServiceEntry {serviceEntry.Id} through {path}-{method}");
            httpContext.SetUserClaims();
            httpContext.SetHttpHandleAddressInfo();
            var sp = Stopwatch.StartNew();
            var parameters = await _parameterParser.Parser(serviceEntry);
            var messageId = GetMessageId(httpContext);
            var serviceKey = await ResolveServiceKey(httpContext);
            var rpcConnection = RpcContext.Context.Connection;
            var clientRpcEndpoint = rpcConnection.ClientHost;
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var serverHandleInfo = serverHandleMonitor?.Monitor((serviceEntry.Id, clientRpcEndpoint));
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetServiceKey(serviceKey);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    $"serviceKey => {serviceKey}");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.RpcToken, _rpcOptions.Token);

            var tracingTimestamp = TracingBefore(new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceId,
                ServiceEntryId = serviceEntry.Id,
                Attachments = RpcContext.Context.GetContextAttachments(),
                Parameters = parameters
            }, messageId, serviceEntry);
            object executeResult = null;
            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                executeResult = await _executor.Execute(serviceEntry, parameters, serviceKey);
                TracingAfter(tracingTimestamp, messageId, serviceEntry, new RemoteResultMessage()
                {
                    ServiceEntryId = serviceEntry.Id,
                    StatusCode = StatusCode.Success,
                    Result = executeResult,
                });
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                TracingError(tracingTimestamp, messageId, serviceEntry, ex.GetExceptionStatusCode(), ex);
                await HandleException(ex);
                Logger.LogException(ex);
                throw;
            }
            finally
            {
                sp.Stop();
                if (isHandleSuccess)
                {
                    serverHandleMonitor?.ExecSuccess((serviceEntry?.Id, clientRpcEndpoint), sp.ElapsedMilliseconds,
                        serverHandleInfo);
                }
                else
                {
                    serverHandleMonitor?.ExecFail((serviceEntry?.Id, clientRpcEndpoint), !isFriendlyStatus,
                        sp.ElapsedMilliseconds, serverHandleInfo);
                }
            }

            await HandleResult(httpContext, executeResult);
        }

        protected abstract Task HandleResult(HttpContext httpContext, object result);


        protected abstract Task HandleException(Exception exception);

        protected virtual Task<string> ResolveServiceKey(HttpContext httpContext)
        {
            return Task.Factory.StartNew(() =>
            {
                string serviceKey = null;
                if (httpContext.Request.Headers.ContainsKey("serviceKey"))
                {
                    serviceKey = httpContext.Request.Headers["serviceKey"].ToString();
                }

                return serviceKey;
            });
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

        private string GetMessageId(HttpContext httpContext)
        {
            httpContext.SetHttpMessageId();
            return httpContext.TraceIdentifier;
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
                    ClientAddress = RpcContext.Context.Connection.ClientAddress,
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }
    }
}