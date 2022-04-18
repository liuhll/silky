using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Http.Core.Executor;
using Silky.Rpc.Auditing;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal class DefaultHttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        private readonly IHttpExecutor _executor;
        private readonly IParameterParser _parameterParser;
        private readonly IHttpHandleDiagnosticListener _httpHandleDiagnosticListener;
        private readonly IAuditSerializer _auditSerializer;

        public DefaultHttpMessageReceivedHandler(
            IHttpExecutor executor,
            ISerializer serializer,
            IParameterParser parameterParser,
            IHttpHandleDiagnosticListener httpHandleDiagnosticListener,
            IAuditSerializer auditSerializer) : base(serializer)
        {
            _executor = executor;
            _parameterParser = parameterParser;
            _httpHandleDiagnosticListener = httpHandleDiagnosticListener;
            _auditSerializer = auditSerializer;

            Logger = NullLogger<DefaultHttpMessageReceivedHandler>.Instance;
        }


        protected override async Task HandleCallAsyncCore(HttpContext httpContext,
            HttpContextServerCallContext serverCallContext, ServiceEntry serviceEntry)
        {
            var sp = Stopwatch.StartNew();
            var parameters =
                await _parameterParser.Parser(httpContext.Request, serviceEntry);
            RpcContext.Context.SetRequestParameters(_auditSerializer.Serialize(parameters));
            var serviceKey = await ResolveServiceKey(httpContext);
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetServiceKey(serviceKey);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    $"serviceKey => {serviceKey}");
            }

            var rpcConnection = RpcContext.Context.Connection;
            var clientRpcEndpoint = rpcConnection.ClientHost;
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var messageId = GetMessageId(httpContext);
            var serverHandleInfo =
                serverHandleMonitor?.Monitor((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint));
            var tracingTimestamp =
                _httpHandleDiagnosticListener.TracingBefore(messageId, serviceEntry,
                    httpContext,
                    parameters);
            object executeResult = null;
            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                executeResult =
                    await _executor.Execute(serviceEntry, parameters, serviceKey);
                var cancellationToken = serverCallContext.HttpContext.RequestAborted;
                if (!serverCallContext.HttpContext.Response.HasStarted && !cancellationToken.IsCancellationRequested)
                {
                    serverCallContext.WriteResponseHeaderCore();
                    if (executeResult != null)
                    {
                        var responseData = _serializer.Serialize(executeResult);
                        await serverCallContext.HttpContext.Response.WriteAsync(responseData);
                    }
                }

                _httpHandleDiagnosticListener.TracingAfter(tracingTimestamp, messageId,
                    serviceEntry,
                    httpContext,
                    executeResult);
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                _httpHandleDiagnosticListener.TracingError(tracingTimestamp, messageId,
                    serviceEntry,
                    httpContext, ex,
                    ex.GetExceptionStatusCode());
                throw;
            }
            finally
            {
                sp.Stop();
                if (isHandleSuccess)
                {
                    serverHandleMonitor?.ExecSuccess((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint),
                        sp.ElapsedMilliseconds,
                        serverHandleInfo);
                }
                else
                {
                    serverHandleMonitor?.ExecFail((serverCallContext.ServiceEntryDescriptor?.Id, clientRpcEndpoint),
                        !isFriendlyStatus,
                        sp.ElapsedMilliseconds, serverHandleInfo);
                }
            }
        }

        protected override async Task HandleCallAsyncCore(HttpContext httpContext,
            HttpContextServerCallContext serverCallContext,
            ServiceEntryDescriptor serviceEntryDescriptor)
        {
            var sp = Stopwatch.StartNew();
            var parameters =
                await _parameterParser.Parser(httpContext.Request, serviceEntryDescriptor);
            var serviceKey = await ResolveServiceKey(httpContext);
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetServiceKey(serviceKey);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    $"serviceKey => {serviceKey}");
            }

            var rpcConnection = RpcContext.Context.Connection;
            var clientRpcEndpoint = rpcConnection.ClientHost;
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var messageId = GetMessageId(httpContext);
            var serverHandleInfo =
                serverHandleMonitor?.Monitor((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint));
            object executeResult = null;
            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                executeResult =
                    await _executor.Execute(serviceEntryDescriptor, parameters, serviceKey);
                var cancellationToken = serverCallContext.HttpContext.RequestAborted;
                if (!serverCallContext.HttpContext.Response.HasStarted && !cancellationToken.IsCancellationRequested)
                {
                    serverCallContext.WriteResponseHeaderCore();
                    if (executeResult != null)
                    {
                        var responseData = _serializer.Serialize(executeResult);
                        await serverCallContext.HttpContext.Response.WriteAsync(responseData);
                    }
                }
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                throw;
            }
            finally
            {
                sp.Stop();
                if (isHandleSuccess)
                {
                    serverHandleMonitor?.ExecSuccess((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint),
                        sp.ElapsedMilliseconds,
                        serverHandleInfo);
                }
                else
                {
                    serverHandleMonitor?.ExecFail((serverCallContext.ServiceEntryDescriptor?.Id, clientRpcEndpoint),
                        !isFriendlyStatus,
                        sp.ElapsedMilliseconds, serverHandleInfo);
                }
            }
        }

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