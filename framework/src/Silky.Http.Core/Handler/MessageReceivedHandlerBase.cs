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
using Silky.Rpc.Runtime.Server;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Runtime.Rpc;
using Silky.Http.Core.Executor;
using Silky.Rpc.Auditing;
using Silky.Rpc.Extensions;
using Silky.Rpc.Security;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        private readonly IHttpExecutor _executor;
        private readonly IParameterParser _parameterParser;
        private readonly ICurrentRpcToken _currentRpcToken;
        private readonly IHttpHandleDiagnosticListener _httpHandleDiagnosticListener;
        private readonly IAuditSerializer _auditSerializer;
        public ILogger<MessageReceivedHandlerBase> Logger { get; set; }

        protected MessageReceivedHandlerBase(
            IHttpExecutor executor,
            IParameterParser parameterParser,
            ICurrentRpcToken currentRpcToken,
            IHttpHandleDiagnosticListener httpHandleDiagnosticListener,
            IAuditSerializer auditSerializer)
        {
            _executor = executor;
            _parameterParser = parameterParser;
            _currentRpcToken = currentRpcToken;
            _httpHandleDiagnosticListener = httpHandleDiagnosticListener;
            _auditSerializer = auditSerializer;

            Logger = NullLogger<MessageReceivedHandlerBase>.Instance;
        }

        public virtual async Task Handle([NotNull] ServiceEntry serviceEntry, [NotNull] HttpContext httpContext)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(httpContext, nameof(httpContext));
            var path = httpContext.Request.Path;
            var method = httpContext.Request.Method.ToEnum<HttpMethod>();
            Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                MiniProfileConstant.Route.State.FindServiceEntry,
                $"Find the ServiceEntry {serviceEntry.Id} through {path}-{method}");
            httpContext.SetUserClaims();
            httpContext.SetHttpHandleAddressInfo();
            var sp = Stopwatch.StartNew();
            var parameters = await _parameterParser.Parser(httpContext.Request, serviceEntry);
            RpcContext.Context.SetRequestParameters(_auditSerializer.Serialize(parameters));
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

            _currentRpcToken.SetRpcToken();
            var tracingTimestamp =
                _httpHandleDiagnosticListener.TracingBefore(messageId, serviceEntry, httpContext, parameters);
            object executeResult = null;
            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                executeResult = await _executor.Execute(serviceEntry, parameters, serviceKey);
                _httpHandleDiagnosticListener.TracingAfter(tracingTimestamp, messageId, serviceEntry, httpContext,
                    executeResult);
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                _httpHandleDiagnosticListener.TracingError(tracingTimestamp, messageId, serviceEntry, httpContext, ex,
                    ex.GetExceptionStatusCode());
                await HandleException(httpContext, ex);
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


        protected abstract Task HandleException(HttpContext httpContext, Exception exception);

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


        private string GetMessageId(HttpContext httpContext)
        {
            httpContext.SetHttpMessageId();
            return httpContext.TraceIdentifier;
        }
    }
}