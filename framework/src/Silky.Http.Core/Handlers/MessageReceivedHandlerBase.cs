using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Options;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Transport.Messages;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IExecutor _executor;

        protected RpcOptions _rpcOptions;

        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);

        protected MessageReceivedHandlerBase(
            IOptionsMonitor<RpcOptions> rpcOptions,
            IExecutor executor)
        {
            _executor = executor;
            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);
        }

        public virtual async Task Handle(ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var sp = Stopwatch.StartNew();
            var parameters = await ResolveParameters(serviceEntry);
            var serviceKey = await ResolveServiceKey();
            RpcContext.Context.SetAttachment(AttachmentKeys.RpcToken, _rpcOptions.Token);
            var tracingTimestamp = TracingBefore(new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceId,
                ServiceEntryId = serviceEntry.Id,
                Attachments = RpcContext.Context.GetContextAttachments(),
                Parameters = parameters
            }, serviceEntry);
            object executeResult = null;
            try
            {
                executeResult = await _executor.Execute(serviceEntry, parameters, serviceKey);
                TracingAfter(tracingTimestamp, serviceEntry, new RemoteResultMessage()
                {
                    ServiceEntryId = serviceEntry.Id,
                    StatusCode = StatusCode.Success,
                    Result = executeResult,
                });
            }
            catch (Exception e)
            {
                TracingError(tracingTimestamp, serviceEntry, e.GetExceptionStatusCode(), e);
                await HandleException(e);
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

        private long? TracingBefore(RemoteInvokeMessage message, ServiceEntry serviceEntry)
        {
            if (serviceEntry.IsLocal && s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcRequest))
            {
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = RpcContext.Context.GetMessageId(),
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntryId = message.ServiceEntryId,
                    Message = message
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcRequest, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, ServiceEntry serviceEntry,
            RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeResultEventData()
                {
                    MessageId = RpcContext.Context.GetMessageId(),
                    ServiceEntryId = serviceEntry.Id,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcRequest, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, ServiceEntry serviceEntry,
            StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeExceptionEventData()
                {
                    MessageId = RpcContext.Context.GetMessageId(),
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