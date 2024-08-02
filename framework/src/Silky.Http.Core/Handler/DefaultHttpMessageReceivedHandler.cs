using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Executor;
using Silky.Rpc.Auditing;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal sealed class DefaultHttpMessageReceivedHandler : MessageReceivedHandlerBase
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
            IAuditSerializer auditSerializer, IOptionsMonitor<GatewayOptions> gatewayOptionsMonitor) : base(serializer,
            gatewayOptionsMonitor)
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
            var messageId = GetMessageId(httpContext);
            var tracingTimestamp =
                _httpHandleDiagnosticListener.TracingBefore(messageId, serviceEntry.Id, serviceEntry.IsLocal,
                    httpContext,
                    parameters);
            var serviceKey = ResolveServiceKey(httpContext);
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetServiceKey(serviceKey);
                Logger.LogInformation(
                    $"serviceKey => {serviceKey} for serviceEntryId {serviceEntry.Id}");
            }

            var rpcConnection = RpcContext.Context.Connection;
            var clientRpcEndpoint = rpcConnection.ClientHost;
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var serverHandleInfo =
                serverHandleMonitor?.Monitor((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint));

            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                var executeResult = await _executor.Execute(serviceEntry, parameters, serviceKey);

                var cancellationToken = serverCallContext.HttpContext.RequestAborted;
                if (!serverCallContext.HttpContext.Response.HasStarted && !cancellationToken.IsCancellationRequested)
                {
                    serverCallContext.WriteResponseHeaderCore();
                    if (executeResult != null)
                    {
                        if (executeResult is IActionResult actionResult)
                        {
                            await actionResult.ExecuteResultAsync(new ActionContext()
                            {
                                HttpContext = serverCallContext.HttpContext
                            });
                        }
                        else
                        {
                            string responseData;
                            if (executeResult is string || executeResult.GetType().IsSample())
                            {
                                responseData = executeResult?.ToString() ?? string.Empty;
                            }
                            else
                            {
                                responseData = _serializer.Serialize(executeResult);
                            }

                            await serverCallContext.HttpContext.Response.WriteAsync(responseData,
                                cancellationToken: cancellationToken);
                        }
                    }
                }

                _httpHandleDiagnosticListener.TracingAfter(tracingTimestamp, messageId,
                    serviceEntry.Id,
                    serviceEntry.IsLocal,
                    httpContext,
                    executeResult);
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                _httpHandleDiagnosticListener.TracingError(tracingTimestamp, messageId,
                    serviceEntry.Id,
                    serviceEntry.IsLocal,
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
            RpcContext.Context.SetRequestParameters(
                _auditSerializer.Serialize(parameters.Where(k => k.Key != ParameterFrom.Header)));
            var messageId = GetMessageId(httpContext);
            var tracingTimestamp =
                _httpHandleDiagnosticListener.TracingBefore(messageId, serviceEntryDescriptor.Id, false,
                    httpContext,
                    parameters);
            var serviceKey = ResolveServiceKey(httpContext);
            if (!serviceKey.IsNullOrEmpty())
            {
                RpcContext.Context.SetServiceKey(serviceKey);
                Logger.LogInformation(
                    $"serviceKey => {serviceKey} for serviceEntryId {serviceEntryDescriptor.Id}");
            }

            var clientRpcEndpoint = RpcContext.Context.Connection.ClientHost;
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var serverHandleInfo =
                serverHandleMonitor?.Monitor((serverCallContext.ServiceEntryDescriptor.Id, clientRpcEndpoint));
            var isHandleSuccess = true;
            var isFriendlyStatus = false;
            try
            {
                var executeResult = await _executor.Execute(serviceEntryDescriptor, parameters, serviceKey);
                var cancellationToken = serverCallContext.HttpContext.RequestAborted;
                if (!serverCallContext.HttpContext.Response.HasStarted && !cancellationToken.IsCancellationRequested)
                {
                    serverCallContext.WriteResponseHeaderCore();
                    if (executeResult != null)
                    {
                        if (executeResult is IActionResult actionResult)
                        {
                            await actionResult.ExecuteResultAsync(new ActionContext()
                            {
                                HttpContext = serverCallContext.HttpContext
                            });
                        }
                        else
                        {
                            string responseData;
                            if (executeResult is string || executeResult.GetType().IsSample())
                            {
                                responseData = executeResult?.ToString() ?? string.Empty;
                            }
                            else
                            {
                                responseData = _serializer.Serialize(executeResult);
                            }

                            await serverCallContext.HttpContext.Response.WriteAsync(responseData,
                                cancellationToken: cancellationToken);
                        }
                    }
                }

                _httpHandleDiagnosticListener.TracingAfter(tracingTimestamp, messageId,
                    serviceEntryDescriptor.Id,
                    false,
                    httpContext,
                    executeResult);
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                isFriendlyStatus = ex.IsFriendlyException();
                _httpHandleDiagnosticListener.TracingError(tracingTimestamp, messageId,
                    serviceEntryDescriptor.Id,
                    false,
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

        private string ResolveServiceKey(HttpContext httpContext)
        {
            string serviceKey = null;
            if (httpContext.Request.Headers.ContainsKey("serviceKey"))
            {
                serviceKey = httpContext.Request.Headers["serviceKey"].ToString();
            }

            return serviceKey;
        }


        private string GetMessageId(HttpContext httpContext)
        {
            httpContext.SetHttpMessageId();
            return httpContext.TraceIdentifier;
        }
    }
}