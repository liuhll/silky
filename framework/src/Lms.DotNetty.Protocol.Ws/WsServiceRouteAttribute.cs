using System;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace Lms.DotNetty.Protocol.Ws
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class WsServiceRouteAttribute : ServiceRouteAttribute
    {
        public WsServiceRouteAttribute(
            string template = "api/{appservice}",
            bool multipleServiceKey = false)
        {
            Template = template;
            MultipleServiceKey = multipleServiceKey;
        }

        public string Template { get; }

        public override ServiceProtocol ServiceProtocol { get; } = ServiceProtocol.Ws;

        public bool MultipleServiceKey { get; }
    }
}