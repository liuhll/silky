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
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Extensions;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteInvoker : IRemoteInvoker
    {
        private readonly IServerManager _serverManager;
        private readonly IInvokeSupervisor _invokeSupervisor;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ISerializer _serializer;
        public ILogger<DefaultRemoteInvoker> Logger { get; set; }

        public DefaultRemoteInvoker(IServerManager serverManager,
            IInvokeSupervisor invokeSupervisor,
            ITransportClientFactory transportClientFactory,
            ISerializer serializer)
        {
            _serverManager = serverManager;
            _invokeSupervisor = invokeSupervisor;
            _transportClientFactory = transportClientFactory;
            _serializer = serializer;
            Logger = NullLogger<DefaultRemoteInvoker>.Instance;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            ShuntStrategy shuntStrategy, string hashKey = null)
        {
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Start,
                $"The rpc request call start{Environment.NewLine} " +
                $"serviceEntryId:[{remoteInvokeMessage.ServiceEntryId}]");
            var rpcEndpoints = FindRpcEndpoint(remoteInvokeMessage);
            var selectedRpcEndpoint =
                SelectedRpcEndpoint(rpcEndpoints, shuntStrategy, remoteInvokeMessage.ServiceEntryId, hashKey);

            var sp = Stopwatch.StartNew();
            RemoteResultMessage invokeResult = null;
            var filters = EngineContext.Current.ResolveAll<IClientFilter>().OrderBy(p => p.Order).ToArray();
            try
            {
                _invokeSupervisor.Monitor((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint));
                RpcContext.Context.SetRcpInvokeAddressInfo(selectedRpcEndpoint.Descriptor);

                var client = await _transportClientFactory.GetClient(selectedRpcEndpoint);
                foreach (var filter in filters)
                {
                    filter.OnActionExecuting(remoteInvokeMessage);
                }

                invokeResult = await client.SendAsync(remoteInvokeMessage);

                foreach (var filter in filters)
                {
                    filter.OnActionExecuted(invokeResult);
                }

                if (invokeResult != null && invokeResult?.StatusCode != StatusCode.Success)
                {
                    throw new SilkyException(invokeResult.ExceptionMessage, invokeResult.StatusCode);
                }
            }
            catch (Exception ex)
            {
                sp.Stop();
                _invokeSupervisor.ExecFail((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                    sp.Elapsed.TotalMilliseconds);
                Logger.LogException(ex);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Fail,
                    $"The rpc request call failed");
                throw;
            }

            sp.Stop();
            _invokeSupervisor.ExecSuccess((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                sp.Elapsed.TotalMilliseconds);
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.Success,
                $"The rpc request call succeeded");
            return invokeResult;
        }

        private IRpcEndpoint[] FindRpcEndpoint(RemoteInvokeMessage remoteInvokeMessage)
        {
            var rpcEndpoints = _serverManager.GetRpcEndpoints(remoteInvokeMessage.ServiceId, ServiceProtocol.Tcp);
            if (rpcEndpoints == null)
            {
                throw new NotFindServiceRouteException(
                    $"The service routing could not be found via [{remoteInvokeMessage.ServiceEntryId}]",
                    StatusCode.NotFindServiceRoute);
            }

            if (!rpcEndpoints.Any(p => p.Enabled))
            {
                throw new NotFindServiceRouteAddressException(
                    $"No available service provider can be found via [{remoteInvokeMessage.ServiceEntryId}]");
            }

            return rpcEndpoints;
        }

        private IRpcEndpoint SelectedRpcEndpoint(IRpcEndpoint[] rpcEndpoints, ShuntStrategy shuntStrategy,
            string serviceEntryId, string hashKey)
        {
            var remoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.SelectedServerEndpoint)?.ToString();
            IRpcEndpoint selectedRpcEndpoint;
            if (remoteAddress != null)
            {
                selectedRpcEndpoint =
                    rpcEndpoints.FirstOrDefault(p =>
                        p.IPEndPoint.ToString().Equals(remoteAddress) && p.Enabled);
                if (selectedRpcEndpoint == null)
                {
                    throw new NotFindServiceRouteAddressException(
                        $"Server [{serviceEntryId}] does not have a healthy designated service rpcEndpoint [{remoteAddress}]");
                }
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
                $"There are currently available service provider addresses:{_serializer.Serialize(rpcEndpoints.Where(p => p.Enabled).Select(p => p.ToString()))}{Environment.NewLine}" +
                $"The selected service provider rpcEndpoint is:[{selectedRpcEndpoint.ToString()}]");
            return selectedRpcEndpoint;
        }
    }
}