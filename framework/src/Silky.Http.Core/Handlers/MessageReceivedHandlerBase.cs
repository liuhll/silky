using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Options;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Transport.Messages;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IExecutor _executor;
        protected RpcOptions _rpcOptions;
        public ILogger<MessageReceivedHandlerBase> Logger { get; set; }


        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);

        protected MessageReceivedHandlerBase(
            IOptionsMonitor<RpcOptions> rpcOptions,
            IExecutor executor)
        {
            _executor = executor;
            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);
            Logger = NullLogger<MessageReceivedHandlerBase>.Instance;
        }

        public virtual async Task Handle(ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var sp = Stopwatch.StartNew();
            var parameters = await ResolveParameters(serviceEntry);
            var messageId = GetMessageId();
            var serviceKey = await ResolveServiceKey();
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
                TracingError(tracingTimestamp, messageId, serviceEntry, ex.GetExceptionStatusCode(), ex);
                await HandleException(ex);
                Logger.LogException(ex);
                throw;
            }
            finally
            {
                sp.Stop();
            }

            await HandleResult(executeResult);
        }

        protected abstract Task HandleResult(object result);


        protected abstract Task HandleException(Exception exception);

        protected abstract Task<string> ResolveServiceKey();


        protected abstract Task<object[]> ResolveParameters(ServiceEntry serviceEntry);

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

        protected abstract string GetMessageId();


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
                    ClientAddress = RpcContext.Context.GetAttachment(AttachmentKeys.SelectedServerHost).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }
    }
}