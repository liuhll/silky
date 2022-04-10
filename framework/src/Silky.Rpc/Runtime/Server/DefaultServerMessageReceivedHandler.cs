using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Security;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerMessageReceivedHandler : IServerMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        private readonly IServiceKeyExecutor _serviceKeyExecutor;

        public ILogger<DefaultServerMessageReceivedHandler> Logger { get; set; }

        public DefaultServerMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator,
            IServiceKeyExecutor serviceKeyExecutor)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _serviceKeyExecutor = serviceKeyExecutor;
        }

        public async Task<RemoteResultMessage> Handle(RemoteInvokeMessage message, Context context,
            CancellationToken cancellationToken)

        {
            var sp = Stopwatch.StartNew();
            var messageId = RpcContext.Context.GetMessageId();
            var rpcConnection = RpcContext.Context.Connection;
            var clientRpcEndpoint = rpcConnection.ClientAddress;
            Logger.LogDebug(
                "Received a request from the client [{0}].{1}messageId:[{2}],serviceEntryId:[{3}]", clientRpcEndpoint,
                Environment.NewLine, messageId, message.ServiceEntryId);
            var serviceEntry =
                _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceEntryId);
            var serverHandleMonitor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var serverHandleInfo = serverHandleMonitor?.Monitor((serviceEntry.Id, clientRpcEndpoint));
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


                var tokenValidator = EngineContext.Current.Resolve<ITokenValidator>();
                if (!tokenValidator.Validate())
                {
                    throw new RpcAuthenticationException("rpc token is illegal");
                }

                context[PollyContextNames.ServiceEntry] = serviceEntry;

                var parameterResolver =
                    EngineContext.Current.ResolveNamed<IParameterResolver>(message.ParameterType.ToString());
                
                var parameters = parameterResolver.Parser(serviceEntry, message);

                var result = await serviceEntry.Executor(_serviceKeyExecutor.ServiceKey, parameters);

                remoteResultMessage.Result = result;
                remoteResultMessage.Status = (int)StatusCode.Success;
                remoteResultMessage.StatusCode = StatusCode.Success;
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                context[PollyContextNames.Exception] = ex;
                remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
                Logger.LogException(ex);
                throw;
            }
            finally
            {
                sp.Stop();
                context[PollyContextNames.ElapsedTimeMs] = sp.ElapsedMilliseconds;
                if (isHandleSuccess)
                {
                    serverHandleMonitor?.ExecSuccess((serviceEntry?.Id, clientRpcEndpoint), sp.ElapsedMilliseconds,
                        serverHandleInfo);
                }
                else
                {
                    serverHandleMonitor?.ExecFail((serviceEntry?.Id, clientRpcEndpoint),
                        !remoteResultMessage.StatusCode.IsFriendlyStatus(), sp.ElapsedMilliseconds, serverHandleInfo);
                }

                Logger.LogDebug("Server processing completed{0}" +
                                "messageId:[{1}],serviceEntryId:[{2}],handleSuccess:{3}", Environment.NewLine,
                    messageId,
                    message.ServiceEntryId, isHandleSuccess);
                remoteResultMessage.Attachments = RpcContext.Context.GetResultAttachments();
            }

            return remoteResultMessage;
        }
    }
}