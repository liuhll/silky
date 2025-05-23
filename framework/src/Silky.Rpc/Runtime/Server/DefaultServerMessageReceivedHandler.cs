﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerMessageReceivedHandler : IServerMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        private readonly IServiceKeyExecutor _serviceKeyExecutor;

        public ILogger<DefaultServerMessageReceivedHandler> Logger { get; set; }

        private readonly RpcOptions rpcOption;

        public DefaultServerMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator,
            IOptions<RpcOptions> rpcOptions,
            IServiceKeyExecutor serviceKeyExecutor)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _serviceKeyExecutor = serviceKeyExecutor;
            this.rpcOption = rpcOptions.Value;
        }

        public async Task<RemoteResultMessage> Handle(RemoteInvokeMessage message, Context context,
            CancellationToken cancellationToken)

        {
            var sp = Stopwatch.StartNew();
            var messageId = RpcContext.Context.GetMessageId();
            var rpcConnection = RpcContext.Context.Connection;
            var clientUri = rpcConnection.ClientUri;
            Logger.LogDebug(
                "Received a request from the client [{0}].{1}messageId:[{2}],serviceEntryId:[{3}]", clientUri,
                Environment.NewLine, messageId, message.ServiceEntryId);
            var serviceEntry =
                _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceEntryId);

            ServerHandleInfo? serverHandleInfo = null;
            IServerHandleMonitor? serverHandleMonitor = null;
            if (rpcOption.EnableMonitor)
            {
                serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
                serverHandleInfo = serverHandleMonitor?.Monitor((serviceEntry.Id, clientUri));
            }

            var remoteResultMessage = new RemoteResultMessage()
            {
                ServiceEntryId = serviceEntry?.Id
            };
            var isHandleSuccess = true;
            try
            {
                if (serviceEntry == null)
                {
                    throw new NotFindLocalServiceEntryException(
                        $"Failed to get local service entry through serviceEntryId {message.ServiceEntryId}");
                }

                if (serverHandleMonitor != null)
                {
                    var getServerInstanceHandleInfo = await serverHandleMonitor.GetServerInstanceHandleInfo();

                    if (getServerInstanceHandleInfo.AllowMaxConcurrentCount > 0 &&
                        getServerInstanceHandleInfo.ConcurrentCount >
                        getServerInstanceHandleInfo.AllowMaxConcurrentCount)
                    {
                        throw new OverflowMaxServerHandleException(
                            $"Exceeds the maximum allowable processing concurrency. Current concurrency {getServerInstanceHandleInfo.ConcurrentCount}");
                    }
                }


                context[PollyContextNames.ServiceEntry] = serviceEntry;

                var parameterResolver =
                    EngineContext.Current.ResolveNamed<IParameterResolver>(message.ParameterType.ToString());

                var parameters = parameterResolver.Parser(serviceEntry, message);

                var result = await serviceEntry.Executor(_serviceKeyExecutor.ServiceKey, parameters);
                remoteResultMessage.Result = result;
                if (result is FileContentResult)
                {
                    remoteResultMessage.IsFile = true;
                }

                remoteResultMessage.StatusCode = StatusCode.Success;
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                if (ex is ValidationException validationException)
                {
                    Logger.LogWarning(ex.Message);
                    remoteResultMessage.ValidateErrors = validationException.ValidationErrors.ToArray();
                    remoteResultMessage.ExceptionMessage = ex.Message;
                    remoteResultMessage.StatusCode = ex.GetExceptionStatusCode();
                }

                else if (ex.IsFriendlyException())
                {
                    Logger.LogWarning(ex.Message);
                    remoteResultMessage.ExceptionMessage = ex.Message;
                    remoteResultMessage.StatusCode = ex.GetExceptionStatusCode();
                }

                else
                {
                    Logger.LogException(ex);
                    context[PollyContextNames.Exception] = ex;
                    throw;
                }
            }
            finally
            {
                sp.Stop();
                context[PollyContextNames.ElapsedTimeMs] = sp.ElapsedMilliseconds;
                if (isHandleSuccess)
                {
                    serverHandleMonitor?.ExecSuccess((serviceEntry?.Id, clientUri), sp.ElapsedMilliseconds,
                        serverHandleInfo);
                }
                else
                {
                    serverHandleMonitor?.ExecFail((serviceEntry?.Id, clientUri),
                        !remoteResultMessage.StatusCode.IsFriendlyStatus(), sp.ElapsedMilliseconds, serverHandleInfo);
                }

                Logger.LogDebug("Server processing completed{0}" +
                                "messageId:[{1}],serviceEntryId:[{2}],handleSuccess:{3}", Environment.NewLine,
                    messageId,
                    message.ServiceEntryId, isHandleSuccess);
                remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
                remoteResultMessage.TransAttachments = RpcContext.Context.GetTransAttachments();
            }

            return remoteResultMessage;
        }
    }
}
