using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Endpoint
{
    public static class SilkyEndpointHelper
    {
        private const string ANYHOST = "0.0.0.0";
        private const string LOCAL_IP_PATTERN = "127(\\.\\d{1,3}){3}$";
        private const string LOCAL_HOSTADRRESS = "localhost";
        private const string IP_PATTERN = "\\d{1,3}(\\.\\d{1,3}){3,5}$";

        private static ConcurrentDictionary<string, string> _ipCache = new();
        
        public static string GetHostIp(string hostAddress)
        {
            if (_ipCache.TryGetValue($"HostIp_{hostAddress}", out var hostIp))
            {
                return hostIp;
            }

            var result = hostAddress;
            if ((!IsValidAddress(hostAddress) && !IsLocalHost(hostAddress)) || IsAnyHost(hostAddress))
            {
                result = HostAddressUtil.GetLocalHostAnyIp();
            }

            _ = _ipCache.TryAdd($"HostIp_{hostAddress}", result);
            return result;
        }

        public static SilkyEndpointDescriptor GetLocalWebEndpointDescriptor()
        {
            return GetLocalWebEndpoint()?.Descriptor;
        }

        public static ISilkyEndpoint GetLocalWebEndpoint()
        {
            var server = EngineContext.Current.Resolve<IServer>();

            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
            if (address.IsNullOrEmpty())
            {
                throw new SilkyException("Failed to obtain http service rpcEndpoint");
            }

            var addressDescriptor = ParserSilkyEndpoint(address);
            return addressDescriptor;
        }

        private static ISilkyEndpoint ParserSilkyEndpoint(string address)
        {
            var urlInfo = UrlUtil.Parser(address);
            var serviceProtocol = ServiceProtocolUtil.GetServiceProtocol(urlInfo.Item1);
            var silkyEndpoint = GetOrCreateSilkyEndpoint(urlInfo.Item2, urlInfo.Item3,serviceProtocol);
            return silkyEndpoint;
        }

        public static ISilkyEndpoint GetHostSilkyEndpoint()
        {
            if (EngineContext.Current.IsRpcServerProvider())
            {
                var localTcpEndpoint = GetLocalRpcEndpoint();
                return localTcpEndpoint;
            }

            if (EngineContext.Current.IsContainHttpCoreModule())
            {
                var localWebEndpoint = GetLocalWebEndpoint();
                if (localWebEndpoint != null)
                {
                    return localWebEndpoint;
                }
            }
            return null;
        }

        public static ISilkyEndpoint GetLocalRpcEndpoint()
        {
            
            var rpcOptions = EngineContext.Current.GetOptionsSnapshot<RpcOptions>();
            var host = GetHostIp(rpcOptions.Host);
            var port = rpcOptions.Port;
            var silkyEndpoint = GetOrCreateSilkyEndpoint(host,port,ServiceProtocol.Rpc);
            return silkyEndpoint;
        }

        public static bool IsLocalRpcAddress(string address)
        {
            var localAddress = GetLocalRpcEndpoint().GetAddress();
            return localAddress.Equals(address) && EngineContext.Current.IsRpcServerProvider();
        }

        public static string GetLocalAddress()
        {
            var host = GetHostIp(ANYHOST);
            return host;
        }


        public static ISilkyEndpoint GetLocalSilkyEndpoint(int port, ServiceProtocol serviceProtocol)
        {
            var host = GetIp(HostAddressUtil.GetLocalHostAnyIp());
            var silkyEndpoint = GetOrCreateSilkyEndpoint(host, port, serviceProtocol);
            return silkyEndpoint;
        }

        public static ISilkyEndpoint GetWsEndpoint()
        {
            var webSocketOptions = EngineContext.Current.GetOptions<WebSocketOptions>();
            var rpcOptions = EngineContext.Current.GetOptions<RpcOptions>();
            var host = GetHostIp(rpcOptions.Host);
            var silkyEndpoint = GetOrCreateSilkyEndpoint(host, webSocketOptions.Port, ServiceProtocol.Ws);
            return silkyEndpoint;
        }

        public static ISilkyEndpoint GetOrCreateSilkyEndpoint(string host, int port, ServiceProtocol serviceProtocol)
        {
            var rpcEndpointMonitor = EngineContext.Current.Resolve<IRpcEndpointMonitor>();
            if (rpcEndpointMonitor.TryGetSilkyEndpoint(host, port, serviceProtocol, out var rpcEndpoint))
            {
                return rpcEndpoint;
            }
            rpcEndpoint = new SilkyEndpoint(host, port, serviceProtocol);
            rpcEndpointMonitor.Monitor(rpcEndpoint);
            return rpcEndpoint;
        }



        public static string GetIp(string host)
        {
            if (IsValidIp(host))
            {
                return host;
            }

            var ips = Dns.GetHostAddresses(host);
            return ips[0].ToString();
        }

        public static bool IsValidIp(string address)
        {
            if (Regex.IsMatch(address, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                string[] ips = address.Split('.');
                if (ips.Length == 4 || ips.Length == 6)
                {
                    if (int.Parse(ips[0]) < 256 && int.Parse(ips[1]) < 256 && int.Parse(ips[2]) < 256 &&
                        int.Parse(ips[3]) < 256)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        private static bool IsValidAddress(string address)
        {
            return (address != null
                    && !ANYHOST.Equals(address)
                    && address.IsMatch(IP_PATTERN));
        }

        private static bool IsAnyHost(String host)
        {
            return ANYHOST.Equals(host);
        }

        private static bool IsLocalHost(string host)
        {
            return host != null
                   && (host.IsMatch(LOCAL_IP_PATTERN)
                       || host.Equals(LOCAL_HOSTADRRESS, StringComparison.OrdinalIgnoreCase));
        }
    }
}