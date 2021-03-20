using System;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace Lms.DotNetty.Protocol.Ws
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class WsServiceRouteAttribute : ServiceRouteAttribute
    {
        public WsServiceRouteAttribute(int wsPort,
            string template = "api/{appservice}",
            bool multipleServiceKey = false)
        {
            RpcPort = wsPort;
            Template = template;
            MultipleServiceKey = multipleServiceKey;
        }

        public string Template { get; }

        public override ServiceProtocol ServiceProtocol { get; } = ServiceProtocol.Ws;

        public override int RpcPort { get; protected set; }

        public bool MultipleServiceKey { get; }
    }
}