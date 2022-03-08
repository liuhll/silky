using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Core.Utils;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteInvoker : IRemoteInvoker
    {
        private readonly IServerManager _serverManager;

        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ISerializer _serializer;
        private readonly ClientFilterProvider _clientFilterProvider;
        private readonly IClientInvokeDiagnosticListener _clientInvokeDiagnosticListener;
        public ILogger<DefaultRemoteInvoker> Logger { get; set; }

        public DefaultRemoteInvoker(IServerManager serverManager,
            ITransportClientFactory transportClientFactory,
            ISerializer serializer,
            ClientFilterProvider clientFilterProvider,
            IClientInvokeDiagnosticListener clientInvokeDiagnosticListener)
        {
            _serverManager = serverManager;

            _transportClientFactory = transportClientFactory;
            _serializer = serializer;
            _clientFilterProvider = clientFilterProvider;
            _clientInvokeDiagnosticListener = clientInvokeDiagnosticListener;

            Logger = NullLogger<DefaultRemoteInvoker>.Instance;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            ShuntStrategy shuntStrategy, string hashKey = null)
        {
            var sp = Stopwatch.StartNew();
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Start,
                "The rpc request call start{0} serviceEntryId:[{1}]",
                args: new[] { Environment.NewLine, remoteInvokeMessage.ServiceEntryId });
            var messageId = GuidGenerator.CreateGuidStrWithNoUnderline();
            var tracingTimestamp = _clientInvokeDiagnosticListener.TracingBefore(remoteInvokeMessage, messageId);
            var rpcEndpoints = FindRpcEndpoint(remoteInvokeMessage);
            var selectedRpcEndpoint =
                SelectedRpcEndpoint(rpcEndpoints, shuntStrategy, remoteInvokeMessage.ServiceEntryId, hashKey,
                    out var confirmedShuntStrategy);
            _clientInvokeDiagnosticListener.TracingSelectInvokeAddress(tracingTimestamp,
                remoteInvokeMessage.ServiceEntryId, confirmedShuntStrategy,
                rpcEndpoints, selectedRpcEndpoint);

            RemoteResultMessage invokeResult = null;
            var invokeMonitor = EngineContext.Current.Resolve<IInvokeMonitor>();
            var filters = _clientFilterProvider.GetClientFilters(remoteInvokeMessage.ServiceEntryId);
            ClientInvokeInfo clientInvokeInfo = null;
            try
            {
                clientInvokeInfo =
                    invokeMonitor?.Monitor((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint));
                var client = await _transportClientFactory.GetClient(selectedRpcEndpoint);
                foreach (var filter in filters)
                {
                    filter.OnActionExecuting(remoteInvokeMessage);
                }

                invokeResult = await client.SendAsync(remoteInvokeMessage, messageId);

                foreach (var filter in filters)
                {
                    filter.OnActionExecuted(invokeResult);
                }
            }
            catch (Exception ex)
            {
                sp.Stop();
                invokeMonitor?.ExecFail((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                    sp.Elapsed.TotalMilliseconds, clientInvokeInfo);

                Logger.LogException(ex);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Fail,
                    $"The rpc request call failed");
                _clientInvokeDiagnosticListener.TracingError(tracingTimestamp, messageId,
                    remoteInvokeMessage.ServiceEntryId, ex.GetExceptionStatusCode(), ex);
                throw;
            }

            sp.Stop();
            invokeMonitor?.ExecSuccess((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                sp.Elapsed.TotalMilliseconds, clientInvokeInfo);
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.Success,
                $"The rpc request call succeeded");
            _clientInvokeDiagnosticListener.TracingAfter(tracingTimestamp, messageId,
                remoteInvokeMessage.ServiceEntryId, invokeResult);
            return invokeResult;
        }

        private IRpcEndpoint[] FindRpcEndpoint(RemoteInvokeMessage remoteInvokeMessage)
        {
            var rpcEndpoints = _serverManager.GetRpcEndpoints(remoteInvokeMessage.ServiceId, ServiceProtocol.Tcp);
            if (rpcEndpoints == null)
            {
                throw new NotFindServiceRouteException(
                    $"The service routing could not be found via [{remoteInvokeMessage.ServiceEntryId}]");
            }

            if (!rpcEndpoints.Any(p => p.Enabled))
            {
                throw new NotFindServiceRouteAddressException(
                    $"No available service provider can be found via [{remoteInvokeMessage.ServiceEntryId}]");
            }

            return rpcEndpoints;
        }

        private IRpcEndpoint SelectedRpcEndpoint(IRpcEndpoint[] rpcEndpoints, ShuntStrategy shuntStrategy,
            string serviceEntryId, string hashKey, out ShuntStrategy confirmedShuntStrategy)
        {
            var remoteAddress = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.SelectedServerEndpoint)
                ?.ToString();
            IRpcEndpoint selectedRpcEndpoint;
            if (remoteAddress != null)
            {
                selectedRpcEndpoint =
                    rpcEndpoints.FirstOrDefault(p => p.GetAddress().Equals(remoteAddress) && p.Enabled);

                if (selectedRpcEndpoint == null)
                {
                    throw new NotFindServiceRouteAddressException(
                        $"Server [{serviceEntryId}] does not have a healthy designated service rpcEndpoint [{remoteAddress}]");
                }

                confirmedShuntStrategy = ShuntStrategy.Appoint;
            }
            else
            {
                var addressSelector =
                    EngineContext.Current.ResolveNamed<IRpcEndpointSelector>(shuntStrategy.ToString());

                selectedRpcEndpoint = addressSelector.Select(new RpcEndpointSelectContext(serviceEntryId,
                    rpcEndpoints,
                    hashKey));
            }

            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.SelectedAddress,
                "There are currently available service provider addresses:{0}{1}" +
                "The selected service provider rpcEndpoint is:[{2}]",
                args: new[]
                {
                    _serializer.Serialize(rpcEndpoints.Where(p => p.Enabled).Select(p => p.ToString())),
                    Environment.NewLine,
                    selectedRpcEndpoint.ToString()
                });
            RpcContext.Context.SetRcpInvokeAddressInfo(selectedRpcEndpoint.Descriptor);
            confirmedShuntStrategy = shuntStrategy;
            return selectedRpcEndpoint;
        }
    }
}