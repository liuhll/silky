using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.MiniProfiler;
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
            GovernanceOptions governanceOptions, string hashKey = null)
        {
            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Start,
                $"Remote call through Rpc framework");
            var serviceRoute = _serviceRouteCache.GetServiceRoute(remoteInvokeMessage.ServiceId);
            if (serviceRoute == null)
            {
                throw new SilkyException(
                    $"The service routing could not be found via {remoteInvokeMessage.ServiceEntryId}",
                    StatusCode.NotFindServiceRoute);
            }

            if (!serviceRoute.Addresses.Any(p => p.Enabled))
            {
                throw new NotFindServiceRouteAddressException(
                    $"No available service provider can be found via {remoteInvokeMessage.ServiceEntryId}");
            }

            var remoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.SelectedAddress)?.ToString();
            IAddressModel selectedAddress;
            if (remoteAddress != null)
            {
                selectedAddress =
                    serviceRoute.Addresses.FirstOrDefault(p =>
                        p.IPEndPoint.ToString().Equals(remoteAddress) && p.Enabled);
                if (selectedAddress == null)
                {
                    throw new NotFindServiceRouteAddressException(
                        $"ServiceRoute does not have a healthy designated service address {remoteAddress}");
                }
            }
            else
            {
                var addressSelector =
                    EngineContext.Current.ResolveNamed<IAddressSelector>(governanceOptions.ShuntStrategy.ToString());

                selectedAddress = addressSelector.Select(new AddressSelectContext(remoteInvokeMessage.ServiceEntryId,
                    serviceRoute.Addresses,
                    hashKey));
            }

            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.SelectedAddress,
                $"There are currently available service provider addresses:{_serializer.Serialize(serviceRoute.Addresses.Where(p => p.Enabled).Select(p => p.ToString()))}," +
                $"The selected service provider address is:{selectedAddress.ToString()}");
            var sp = Stopwatch.StartNew();
            RemoteResultMessage invokeResult = null;

            var filters = EngineContext.Current.ResolveAll<IClientFilter>().OrderBy(p => p.Order).ToArray();
            try
            {
                _requestServiceSupervisor.Monitor((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                    governanceOptions);
                RpcContext.Context.SetRcpInvokeAddressInfo(selectedAddress,NetUtil.GetRpcAddressModel());

                var client = await _transportClientFactory.GetClient(selectedAddress);
                foreach (var filter in filters)
                {
                    filter.OnActionExecuting(remoteInvokeMessage);
                }

                invokeResult =
                    await client.SendAsync(remoteInvokeMessage, governanceOptions.TimeoutMillSeconds);
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
                Logger.LogException(ex);
                _requestServiceSupervisor.ExecFail((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                    sp.Elapsed.TotalMilliseconds);
                MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                    MiniProfileConstant.Rpc.State.Fail,
                    $"rpc remote call failed");
                throw;
            }

            sp.Stop();
            _requestServiceSupervisor.ExecSuccess((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                sp.Elapsed.TotalMilliseconds);
            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.Success,
                $"rpc remote call succeeded");
            return invokeResult;
        }
    }
}