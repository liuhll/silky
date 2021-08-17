using System;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Runtime.Server
{
    public enum ServiceProtocol
    {
        Tcp,
        Mqtt,
        Ws,
        Http
    }

    public static class ServiceProtocolUtil
    {
        public static ServiceProtocol GetServiceProtocol(string scheme)
        {
            ServiceProtocol serviceProtocol;
            if ("http".Equals(scheme, StringComparison.OrdinalIgnoreCase) ||
                "https".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Http;
            }

            if ("tcp".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Tcp;
            }
            else if ("ws".Equals(scheme, StringComparison.OrdinalIgnoreCase) ||
                     "wss".Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                serviceProtocol = ServiceProtocol.Ws;
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
}