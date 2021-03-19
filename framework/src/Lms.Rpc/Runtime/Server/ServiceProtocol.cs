using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Runtime.Server
{
    public enum ServiceProtocol
    {
        Tcp,
        Mqtt,
        Ws,
    }


    public static class ServiceProtocolExtensions
    {
        public static int GetPort(this ServiceProtocol serviceProtocol)
        {
            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
            var port = 0;
            switch (serviceProtocol)
            {
                case ServiceProtocol.Tcp:
                    port = rpcOptions.RpcPort;
                    break;
                case ServiceProtocol.Ws:
                    port = rpcOptions.WsPort;
                    break;
                case ServiceProtocol.Mqtt:
                    port = rpcOptions.MqttPort;
                    break;
                default:
                    throw new LmsException("指定的服务协议类型错误");
            }

            return port;
        }
    }
}