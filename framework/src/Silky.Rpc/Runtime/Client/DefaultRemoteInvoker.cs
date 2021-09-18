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
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Extensions;
using Silky.Rpc.Routing;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Messages;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteInvoker : IRemoteInvoker
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly IRequestServiceSupervisor _requestServiceSupervisor;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ISerializer _serializer;
        public ILogger<DefaultRemoteInvoker> Logger { get; set; }

        public DefaultRemoteInvoker(ServiceRouteCache serviceRouteCache,
            IRequestServiceSupervisor requestServiceSupervisor,
            ITransportClientFactory transportClientFactory,
            IHealthCheck healthCheck,
            ISerializer serializer)
        {
            _serviceRouteCache = serviceRouteCache;
            _requestServiceSupervisor = requestServiceSupervisor;
            _transportClientFactory = transportClientFactory;
            _serializer = serializer;
            Logger = NullLogger<DefaultRemoteInvoker>.Instance;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            ShuntStrategy shuntStrategy, string hashKey = null)
        {
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Start,
                $"The rpc request call start.{Environment.NewLine} serviceEntryId:[{remoteInvokeMessage.ServiceEntryId}]");
            var serviceRoute = FindServiceRoute(remoteInvokeMessage);
            var selectedRpcEndpoint =
                SelectedRpcEndpoint(serviceRoute, shuntStrategy, remoteInvokeMessage.ServiceEntryId, hashKey);

            var sp = Stopwatch.StartNew();
            RemoteResultMessage invokeResult = null;
            var filters = EngineContext.Current.ResolveAll<IClientFilter>().OrderBy(p => p.Order).ToArray();
            try
            {
                _requestServiceSupervisor.Monitor((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint));
                RpcContext.Context.SetRcpInvokeAddressInfo(selectedRpcEndpoint.Descriptor,
                    AddressHelper.GetLocalRpcEndpointDescriptor());

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
                _requestServiceSupervisor.ExecFail((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                    sp.Elapsed.TotalMilliseconds);
                Logger.LogException(ex);
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Fail,
                    $"The rpc request call failed.{Environment.NewLine}");
                throw;
            }

            sp.Stop();
            _requestServiceSupervisor.ExecSuccess((remoteInvokeMessage.ServiceEntryId, selectedRpcEndpoint),
                sp.Elapsed.TotalMilliseconds);
            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.Success,
                $"The rpc request call succeeded.{Environment.NewLine}");
            return invokeResult;
        }

        private ServiceRoute FindServiceRoute(RemoteInvokeMessage remoteInvokeMessage)
        {
            var serviceRoute = _serviceRouteCache.GetServiceRoute(remoteInvokeMessage.ServiceId);
            if (serviceRoute == null)
            {
                throw new NotFindServiceRouteException(
                    $"The service routing could not be found via [{remoteInvokeMessage.ServiceEntryId}]",
                    StatusCode.NotFindServiceRoute);
            }

            if (!serviceRoute.Endpoints.Any(p => p.Enabled))
            {
                throw new NotFindServiceRouteAddressException(
                    $"No available service provider can be found via [{remoteInvokeMessage.ServiceEntryId}]");
            }

            return serviceRoute;
        }

        private IRpcEndpoint SelectedRpcEndpoint(ServiceRoute serviceRoute, ShuntStrategy shuntStrategy,
            string serviceEntryId, string hashKey)
        {
            var remoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.SelectedServerEndpoint)?.ToString();
            IRpcEndpoint selectedRpcEndpoint;
            if (remoteAddress != null)
            {
                selectedRpcEndpoint =
                    serviceRoute.Endpoints.FirstOrDefault(p =>
                        p.IPEndPoint.ToString().Equals(remoteAddress) && p.Enabled);
                if (selectedRpcEndpoint == null)
                {
                    throw new NotFindServiceRouteAddressException(
                        $"ServiceRoute [{serviceRoute.Service.Id}] does not have a healthy designated service rpcEndpoint [{remoteAddress}]");
                }
            }
            else
            {
                var addressSelector =
                    EngineContext.Current.ResolveNamed<IRpcEndpointSelector>(shuntStrategy.ToString());

                selectedRpcEndpoint = addressSelector.Select(new RpcEndpointSelectContext(serviceEntryId,
                    serviceRoute.Endpoints,
                    hashKey));
            }

            Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.SelectedAddress,
                $"There are currently available service provider addresses:{_serializer.Serialize(serviceRoute.Endpoints.Where(p => p.Enabled).Select(p => p.ToString()))}.{Environment.NewLine}" +
                $"The selected service provider rpcEndpoint is:[{selectedRpcEndpoint.ToString()}]");
            return selectedRpcEndpoint;
        }
    }
}