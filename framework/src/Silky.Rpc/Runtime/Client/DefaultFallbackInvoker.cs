using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.MethodExecutor;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultFallbackInvoker : IFallbackInvoker
    {
        public ILogger<DefaultFallbackInvoker> Logger { get; set; }
        private readonly IFallbackDiagnosticListener _fallbackDiagnosticListener;

        public DefaultFallbackInvoker(IFallbackDiagnosticListener fallbackDiagnosticListener)
        {
            _fallbackDiagnosticListener = fallbackDiagnosticListener;
            Logger = NullLogger<DefaultFallbackInvoker>.Instance;
        }

        public async Task<object> Invoke([NotNull] ServiceEntry serviceEntry, object[] parameters)
        {
            parameters = serviceEntry.ConvertParameters(parameters);

            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(serviceEntry.FallbackMethodExecutor, nameof(serviceEntry.FallbackMethodExecutor));
            Check.NotNull(serviceEntry.FallbackProvider, nameof(serviceEntry.FallbackProvider));
            var fallbackTracingTimestamp =
                _fallbackDiagnosticListener.TracingFallbackBefore(serviceEntry.ServiceId,
                    parameters,
                    RpcContext.Context.GetMessageId(),
                    FallbackExecType.Client,
                    serviceEntry.FallbackProvider);
            var instance = EngineContext.Current.Resolve(serviceEntry.FallbackProvider.Type);
            if (instance == null)
            {
                var ex = new NotFindFallbackInstanceException(
                    $"The implementation class of the failed callback was not found;{Environment.NewLine}" +
                    $"Type:{serviceEntry.FallbackProvider.Type.FullName}");
                _fallbackDiagnosticListener.TracingFallbackError(fallbackTracingTimestamp,
                    RpcContext.Context.GetMessageId(), serviceEntry.Id, ex.GetExceptionStatusCode(),
                    ex,
                    serviceEntry.FallbackProvider);
                throw ex;
            }

            object result = null;
            try
            {
                result = await serviceEntry.FallbackMethodExecutor
                    .ExecuteMethodWithDbContextAsync(instance, parameters);
            }
            catch (Exception ex)
            {
                _fallbackDiagnosticListener.TracingFallbackError(fallbackTracingTimestamp,
                    RpcContext.Context.GetMessageId(), serviceEntry.Id, ex.GetExceptionStatusCode(),
                    ex,
                    serviceEntry.FallbackProvider);
                Logger.LogException(ex);
                throw new FallbackException(ex.Message, ex.InnerException);
            }

            _fallbackDiagnosticListener.TracingFallbackAfter(fallbackTracingTimestamp,
                RpcContext.Context.GetMessageId(), serviceEntry.Id, result,
                serviceEntry.FallbackProvider);
            return result;
        }
    }
}