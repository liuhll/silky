using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Lms.Rpc.Address;
using Silky.Lms.Rpc.Address.HealthCheck;
using Silky.Lms.Rpc.Address.Selector;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Routing;
using Silky.Lms.Rpc.Runtime.Client;
using Silky.Lms.Rpc.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.DotNetty
{
    public class DotNettyRemoteServiceInvoker : IRemoteServiceInvoker
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly IRemoteServiceSupervisor _remoteServiceSupervisor;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly IHealthCheck _healthCheck;
        public ILogger<DotNettyRemoteServiceInvoker> Logger { get; set; }

        public DotNettyRemoteServiceInvoker(ServiceRouteCache serviceRouteCache,
            IRemoteServiceSupervisor remoteServiceSupervisor,
            ITransportClientFactory transportClientFactory,
            IHealthCheck healthCheck)
        {
            _serviceRouteCache = serviceRouteCache;
            _remoteServiceSupervisor = remoteServiceSupervisor;
            _transportClientFactory = transportClientFactory;
            _healthCheck = healthCheck;
            Logger = NullLogger<DotNettyRemoteServiceInvoker>.Instance;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            GovernanceOptions governanceOptions, string hashKey = null)
        {
            var serviceRoute = _serviceRouteCache.GetServiceRoute(remoteInvokeMessage.ServiceId);
            if (serviceRoute == null)
            {
                throw new LmsException($"通过{remoteInvokeMessage.ServiceId}找不到服务路由", StatusCode.NotFindServiceRoute);
            }

            if (!serviceRoute.Addresses.Any(p => p.Enabled))
            {
                throw new NotFindServiceRouteAddressException($"通过{remoteInvokeMessage.ServiceId}找不到可用的服务提供者");
            }

            var addressSelector =
                EngineContext.Current.ResolveNamed<IAddressSelector>(governanceOptions.ShuntStrategy.ToString());
            var selectedAddress =
                addressSelector.Select(new AddressSelectContext(remoteInvokeMessage.ServiceId, serviceRoute.Addresses,
                    hashKey));
            bool isInvakeSuccess = true;
            var sp = Stopwatch.StartNew();
            try
            {
                _remoteServiceSupervisor.Monitor((remoteInvokeMessage.ServiceId, selectedAddress),
                    governanceOptions);
                var client = await _transportClientFactory.GetClient(selectedAddress);
                //RpcContext.GetContext().SetAttachment("localAddress", NetUtil.GetRpcAddressModel().ToString());
                RpcContext.GetContext().SetAttachment("remoteAddress", selectedAddress.ToString());
                return await client.SendAsync(remoteInvokeMessage, governanceOptions.ExecutionTimeout);
            }
            catch (IOException ex)
            {
                Logger.LogError($"服务提供者{selectedAddress}不可用,IO异常,原因:{ex.Message}");
                _healthCheck.RemoveAddress(selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (ConnectException ex)
            {
                Logger.LogError($"与服务提供者{selectedAddress}链接异常,原因:{ex.Message}");
                MarkAddressFail(governanceOptions, selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (ChannelException ex)
            {
                Logger.LogError($"与服务提供者{selectedAddress}通信异常,原因:{ex.Message}");
                MarkAddressFail(governanceOptions, selectedAddress);
                isInvakeSuccess = false;
                throw new CommunicatonException(ex.Message, ex.InnerException);
            }
            catch (TimeoutException ex)
            {
                Logger.LogError($"与服务提供者{selectedAddress}执行超时,原因:{ex.Message}");
                MarkAddressFail(governanceOptions, selectedAddress, true);
                isInvakeSuccess = false;
                throw;
            }
            finally
            {
                sp.Stop();
                if (isInvakeSuccess)
                {
                    _remoteServiceSupervisor.ExecSuccess((remoteInvokeMessage.ServiceId, selectedAddress),
                        sp.Elapsed.TotalMilliseconds);
                }
                else
                {
                    _remoteServiceSupervisor.ExceFail((remoteInvokeMessage.ServiceId, selectedAddress),
                        sp.Elapsed.TotalMilliseconds);
                }
            }
        }

        private void MarkAddressFail(GovernanceOptions governanceOptions, IAddressModel selectedAddress,
            bool isTimeoutEx = false)
        {
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