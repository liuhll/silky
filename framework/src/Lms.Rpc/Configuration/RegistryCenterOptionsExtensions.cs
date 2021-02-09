using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Configuration
{
    public static class RegistryCenterOptionsExtensions
    {
        public static string GetRoutePath(this RegistryCenterOptions options, ServiceProtocol serviceProtocol)
        {
            var routePath = string.Empty;
            switch (serviceProtocol)
            {
                case ServiceProtocol.Mqtt:
                    routePath = options.MqttPtah;
                    break;
                case ServiceProtocol.Tcp:
                    routePath = options.RoutePath;
                    break;
                default:
                    throw new LmsException($"暂未实现{serviceProtocol}的通信协议");
            }

            if (routePath.IsNullOrEmpty())
            {
                throw new LmsException($"未配置{serviceProtocol}的服务路由地址");
            }
            return routePath;
        }
    }
}