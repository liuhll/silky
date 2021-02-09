using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Lms.Core.Exceptions;
using Lms.Rpc.Address.HealthCheck;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.DotNetty
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

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage)
        {
            //var serviceRoute = _serviceRouteCache[remoteInvokeMessage.ServiceId];
            var serviceRoute = _serviceRouteCache.GetServiceRoute(remoteInvokeMessage.ServiceId);
            if (serviceRoute == null)
            {
                throw new LmsException($"通过{remoteInvokeMessage.ServiceId}找不到服务路由", StatusCode.NotFindServiceRoute);
            }

            if (!serviceRoute.Addresses.Any())
            {
                throw new LmsException($"通过{remoteInvokeMessage.ServiceId}找不到可用的服务提供者",
                    StatusCode.NotFindServiceRouteAddress);
            }

            // todo 服务地址选择
            // todo 远程调用监视
            // todo 分布式事务
            // todo 远程调用
            // todo 获取调用结果
            var selectedAddress = serviceRoute.Addresses.First();
            try
            {
                var client = await _transportClientFactory.GetClient(selectedAddress);
                return await client.SendAsync(remoteInvokeMessage);
            }
            catch (IOException ex)
            {
                _healthCheck.RemoveAddress(selectedAddress);
                throw;
            }
            catch (ConnectException ex)
            {
                _healthCheck.ChangeHealthStatus(selectedAddress, false);
                throw;
            }
        }
    }
}