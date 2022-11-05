using System;
using Silky.Core.Exceptions;

namespace Silky.Core.Runtime.Rpc
{
    public enum ServiceProtocol
    {
        Rpc,
        Mqtt,
        Ws,
        Wss,
        Http,
        Https,
    }

    public static class ServiceProtocolUtil
    {
        public static ServiceProtocol GetServiceProtocol(string scheme)
        {
            ServiceProtocol serviceProtocol;
            if ("http".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Http;
            }

            else if ("https".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Https;
            }

            else if ("rpc".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Rpc;
            }
            else if ("ws".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Ws;
            }

            else if ("wss".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Wss;
            }
            else if ("mqtt".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Mqtt;
            }
            else
            {
                throw new SilkyException(
                    $"Silky does not currently support this {scheme} type of communication protocol");
            }

            return serviceProtocol;
        }
    }

    public static class ServiceProtocolExtensions
    {
        public static bool IsHttp(this ServiceProtocol serviceProtocol)
        {
            return serviceProtocol == ServiceProtocol.Http || serviceProtocol == ServiceProtocol.Https;
        }

        public static bool IsWs(this ServiceProtocol serviceProtocol)
        {
            return serviceProtocol == ServiceProtocol.Ws || serviceProtocol == ServiceProtocol.Wss;
        }

        public static bool IsRpc(this ServiceProtocol serviceProtocol)
        {
            return serviceProtocol == ServiceProtocol.Rpc;
        }

        public static bool IsMqtt(this ServiceProtocol serviceProtocol)
        {
            return serviceProtocol == ServiceProtocol.Mqtt;
        }
    }
}