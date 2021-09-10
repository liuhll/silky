using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Configuration;
using Silky.Rpc.Messages;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Utils;

namespace Silky.DotNetty
{
    public class DotNettyRemoteServiceInvoker : IRemoteServiceInvoker
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly IRequestServiceSupervisor _requestServiceSupervisor;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly IHealthCheck _healthCheck;
        private readonly ISerializer _serializer;
        public ILogger<DotNettyRemoteServiceInvoker> Logger { get; set; }

        public DotNettyRemoteServiceInvoker(ServiceRouteCache serviceRouteCache,
            IRequestServiceSupervisor requestServiceSupervisor,
            ITransportClientFactory transportClientFactory,
            IHealthCheck healthCheck,
            ISerializer serializer)
        {
            _serviceRouteCache = serviceRouteCache;
            _requestServiceSupervisor = requestServiceSupervisor;
            _transportClientFactory = transportClientFactory;
            _healthCheck = healthCheck;
            _serializer = serializer;
            Logger = NullLogger<DotNettyRemoteServiceInvoker>.Instance;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            GovernanceOptions governanceOptions, string hashKey = null)
        {
            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.Start,
                $"Remote call through Rpc framework");
            var serviceRouteAddresses = _serviceRouteCache.GetServiceAddress(remoteInvokeMessage.ServiceId);
            if (serviceRouteAddresses == null || !serviceRouteAddresses.Any())
            {
                MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                    MiniProfileConstant.Rpc.State.FindServiceRoute,
                    $"The service routing could not be found via {remoteInvokeMessage.ServiceEntryId}", true);
                throw new SilkyException(
                    $"The service routing could not be found via {remoteInvokeMessage.ServiceEntryId}",
                    StatusCode.NotFindServiceRoute);
            }

            if (!serviceRouteAddresses.Any(p => p.Enabled))
            {
                MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                    MiniProfileConstant.Rpc.State.FindServiceRoute,
                    $"No available service provider can be found via {remoteInvokeMessage.ServiceEntryId}", true);
                throw new NotFindServiceRouteAddressException(
                    $"No available service provider can be found via {remoteInvokeMessage.ServiceEntryId}");
            }

            var remoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.SelectedAddress)?.ToString();
            IAddressModel selectedAddress;
            if (remoteAddress != null)
            {
                selectedAddress =
                    serviceRouteAddresses.FirstOrDefault(p =>
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
                    serviceRouteAddresses, hashKey));
            }

            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.SelectedAddress,
                $"There are currently available service provider addresses:{_serializer.Serialize(serviceRouteAddresses.Where(p => p.Enabled).Select(p => p.ToString()))}," +
                $"The selected service provider address is:{selectedAddress.ToString()}");
            bool isInvakeSuccess = true;
            var sp = Stopwatch.StartNew();
            try
            {
                _requestServiceSupervisor.Monitor((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                    governanceOptions);
                var client = await _transportClientFactory.GetClient(selectedAddress);
                RpcContext.Context
                    .SetAttachment(AttachmentKeys.ServerAddress, selectedAddress.IPEndPoint.ToString());
                RpcContext.Context.SetAttachment(AttachmentKeys.ClientAddress,
                    NetUtil.GetRpcAddressModel().IPEndPoint.ToString());
                return await client.SendAsync(remoteInvokeMessage, governanceOptions.ExecutionTimeout);
            }
            catch (IOException ex)
            {
                Logger.LogError(
                    $"IO exception, Service provider {selectedAddress} is unavailable,  reason: {ex.Message}");
                _healthCheck.RemoveAddress(selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (ConnectException ex)
            {
                Logger.LogError(
                    $"The link with the service provider {selectedAddress} is abnormal, the reason: {ex.Message}");
                _healthCheck.RemoveAddress(selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (ChannelException ex)
            {
                Logger.LogError(
                    $"Abnormal communication with service provider {selectedAddress}, reason: {ex.Message}");
                _healthCheck.RemoveAddress(selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (TimeoutException ex)
            {
                Logger.LogError($"Execution timed out with service provider {selectedAddress}, reason: {ex.Message}");
                MarkAddressFail(governanceOptions, selectedAddress, ex, true);
                isInvakeSuccess = false;
                throw;
            }
            catch (Exception ex)
            {
                if (!ex.IsBusinessException() && !ex.IsUnauthorized())
                {
                    MiniProfilerPrinter.Print(MiniProfileConstant.RemoteInvoker.Name,
                        MiniProfileConstant.RemoteInvoker.State.Fail,
                        $"{ex.Message}", true);
                }

                if (ex is NotFindLocalServiceEntryException ||
                    ex.GetExceptionStatusCode() == StatusCode.NotFindLocalServiceEntry)
                {
                    throw new NotFindLocalServiceEntryException(ex.Message);
                }

                throw;
            }
            finally
            {
                sp.Stop();
                if (isInvakeSuccess)
                {
                    _requestServiceSupervisor.ExecSuccess((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                        sp.Elapsed.TotalMilliseconds);
                    MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                        MiniProfileConstant.Rpc.State.Success,
                        $"rpc remote call succeeded");
                }
                else
                {
                    _requestServiceSupervisor.ExecFail((remoteInvokeMessage.ServiceEntryId, selectedAddress),
                        sp.Elapsed.TotalMilliseconds);
                    MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                        MiniProfileConstant.Rpc.State.Fail,
                        $"rpc remote call failed");
                }
            }
        }

        private void MarkAddressFail(GovernanceOptions governanceOptions, IAddressModel selectedAddress, Exception ex,
            bool isTimeoutEx = false)
        {
            MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name,
                MiniProfileConstant.Rpc.State.MarkAddressFail,
                $"Failed to call remote service using address {selectedAddress}, reason: {ex.Message}", true);
            if (governanceOptions.FuseProtection)
            {
                selectedAddress.MakeFusing(governanceOptions.FuseSleepDuration);
                if (selectedAddress.FuseTimes > governanceOptions.FuseTimes && !isTimeoutEx)
                {
                    _healthCheck.ChangeHealthStatus(selectedAddress, false, governanceOptions.FuseTimes);
                }
            }
            else if (!isTimeoutEx)
            {
                _healthCheck.ChangeHealthStatus(selectedAddress, false);
            }
        }
    }
}