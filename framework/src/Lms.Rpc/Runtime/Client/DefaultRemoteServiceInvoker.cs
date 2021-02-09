using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceInvoker : IRemoteServiceInvoker
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly IRemoteServiceSupervisor _remoteServiceSupervisor;
        private readonly ITransportClientFactory _transportClientFactory;

        public DefaultRemoteServiceInvoker(ServiceRouteCache serviceRouteCache,
            IRemoteServiceSupervisor remoteServiceSupervisor,
            ITransportClientFactory transportClientFactory)
        {
            _serviceRouteCache = serviceRouteCache;
            _remoteServiceSupervisor = remoteServiceSupervisor;
            _transportClientFactory = transportClientFactory;
        }

        public async Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage)
        {
            var serviceRoute = _serviceRouteCache[remoteInvokeMessage.ServiceId];
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
            var client = await _transportClientFactory.CreateClientAsync(selectedAddress.IPEndPoint);
            return await client.SendAsync(remoteInvokeMessage);
        }
    }
}