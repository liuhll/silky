using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Auditing;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerFallbackHandler : IServerFallbackHandler
    {
        public ILogger<DefaultServerFallbackHandler> Logger { get; set; }
        private readonly IServerDiagnosticListener _serverDiagnosticListener;
        private readonly IFallbackDiagnosticListener _fallbackDiagnosticListener;

        public DefaultServerFallbackHandler(IServerDiagnosticListener serverDiagnosticListener,
            IFallbackDiagnosticListener fallbackDiagnosticListener)
        {
            _serverDiagnosticListener = serverDiagnosticListener;
            _fallbackDiagnosticListener = fallbackDiagnosticListener;
            Logger = NullLogger<DefaultServerFallbackHandler>.Instance;
        }

        public async Task<RemoteResultMessage> Handle(RemoteInvokeMessage message, Context ctx,
            CancellationToken cancellationToken)
        {
            var tracingTimestamp = ctx[PollyContextNames.TracingTimestamp]?.To<long>();
            var remoteResultMessage = new RemoteResultMessage()
            {
                ServiceEntryId = message.ServiceEntryId,
                StatusCode = StatusCode.Success,
            };

            var exception = ctx[PollyContextNames.Exception] as Exception;
            Check.NotNull(exception, nameof(exception));
            _serverDiagnosticListener.TracingError(tracingTimestamp, RpcContext.Context.GetMessageId(),
                message.ServiceEntryId, exception.GetExceptionStatusCode(), exception);
            if (exception is RpcAuthenticationException || exception is NotFindLocalServiceEntryException)
            {
                remoteResultMessage.StatusCode = exception.GetExceptionStatusCode();
                remoteResultMessage.ExceptionMessage = exception.Message;
                return remoteResultMessage;
            }

            if (!ctx.TryGetValue(PollyContextNames.ServiceEntry, out var ctxValue))
            {
                remoteResultMessage.StatusCode = exception.GetExceptionStatusCode();
                remoteResultMessage.ExceptionMessage = exception.GetExceptionMessage();
                return remoteResultMessage;
            }

            var serviceEntry = ctxValue as ServiceEntry;
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            if (serviceEntry.FallbackMethodExecutor != null && serviceEntry.FallbackProvider != null)
            {
                var fallbackTracingTimestamp =
                    _fallbackDiagnosticListener.TracingFallbackBefore(message.ServiceEntryId, message.Parameters,
                        RpcContext.Context.GetMessageId(),
                        FallbackExecType.Server,
                        serviceEntry.FallbackProvider);
                object instance = EngineContext.Current.Resolve(serviceEntry.FallbackProvider.Type);
                if (instance == null)
                {
                    remoteResultMessage.StatusCode = StatusCode.NotFindFallbackInstance;
                    remoteResultMessage.ExceptionMessage =
                        $"Failed to instantiate the instance of the fallback service;{Environment.NewLine}" +
                        $"Type:{serviceEntry.FallbackProvider.Type.FullName}";
                    _fallbackDiagnosticListener.TracingFallbackError(fallbackTracingTimestamp,
                        RpcContext.Context.GetMessageId(), serviceEntry.Id, remoteResultMessage.StatusCode,
                        new NotFindFallbackInstanceException(
                            "Failed to instantiate the instance of the fallback service."),
                        serviceEntry.FallbackProvider);
                    return remoteResultMessage;
                }

                object result = null;
                try
                {
                    var parameters = serviceEntry.ConvertParameters(message.Parameters);
                    result = await serviceEntry.FallbackMethodExecutor.ExecuteMethodWithAuditingAsync(instance,
                        parameters, serviceEntry);

                    remoteResultMessage.StatusCode = StatusCode.Success;
                    remoteResultMessage.Result = result;
                    remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
                    _fallbackDiagnosticListener.TracingFallbackAfter(fallbackTracingTimestamp,
                        RpcContext.Context.GetMessageId(), serviceEntry.Id, result,
                        serviceEntry.FallbackProvider);
                    return remoteResultMessage;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    remoteResultMessage.StatusCode = ex.GetExceptionStatusCode();
                    remoteResultMessage.ExceptionMessage = ex.GetExceptionMessage();
                    _fallbackDiagnosticListener.TracingFallbackError(fallbackTracingTimestamp,
                        RpcContext.Context.GetMessageId(), serviceEntry.Id, ex.GetExceptionStatusCode(),
                        ex, serviceEntry.FallbackProvider);
                    return remoteResultMessage;
                }
                finally
                {
                    remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
                }
            }

            remoteResultMessage.StatusCode = exception.GetExceptionStatusCode();
            remoteResultMessage.ExceptionMessage = exception.GetExceptionMessage();
            remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
            return remoteResultMessage;
        }
    }
}