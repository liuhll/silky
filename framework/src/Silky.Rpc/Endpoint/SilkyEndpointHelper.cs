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
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Endpoint
{
    public static class SilkyEndpointHelper
    {
        private const string ANYHOST = "0.0.0.0";
        private const string LOCAL_IP_PATTERN = "127(\\.\\d{1,3}){3}$";
        private const string LOCAL_HOSTADRRESS = "localhost";
        private const string IP_PATTERN = "\\d{1,3}(\\.\\d{1,3}){3,5}$";

        private static ConcurrentDictionary<string, string> _ipCache = new();

        private static ConcurrentDictionary<string, ISilkyEndpoint> _rpcEndpointCache = new();

        public static string GetHostIp(string hostAddress)
        {
            if (_ipCache.TryGetValue($"HostIp_{hostAddress}", out var hostIp))
            {
                return hostIp;
            }

            var result = hostAddress;
            if ((!IsValidAddress(hostAddress) && !IsLocalHost(hostAddress)) || IsAnyHost(hostAddress))
            {
                result = GetAnyHostIp();
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
                //throw new SilkyException("Failed to obtain http service rpcEndpoint");
                return null;
            }

            var addressDescriptor = ParseRpcEndpointDescriptor(address);
            return addressDescriptor;
        }

        private static ISilkyEndpoint ParseRpcEndpointDescriptor(string address)
        {
            var addressSegments = address.Split("://");
            var scheme = addressSegments.First();
            var url = addressSegments.Last();
            var serviceProtocol = ServiceProtocolUtil.GetServiceProtocol(scheme);

            string host;

            if (url.Contains("+") || url.Contains("[::]") || url.Contains("*"))
            {
                host = GetAnyHostIp();
            }
            else
            {
                var domainAndPort = url.Split(":");
                host = domainAndPort[0];
            }

            var port = url.Split(":").Last().To<int>();
            return new SilkyEndpoint(host, port, serviceProtocol);
        }

        private static string GetAnyHostIp()
        {
            var result = "";
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;
                IPInterfaceProperties pix = adapter.GetIPProperties();
                if (pix.GatewayAddresses.Count == 0)
                    continue;
                UnicastIPAddressInformationCollection ipCollection = pix.UnicastAddresses;
                foreach (UnicastIPAddressInformation ipaddr in ipCollection)
                {
                    if (ipaddr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result = ipaddr.Address.ToString();
                        break;
                    }
                }
            }

            if (!result.IsNullOrEmpty())
            {
                return result;
            }

            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }

        public static ISilkyEndpoint GetLocalRpcEndpoint()
        {
            if (_rpcEndpointCache.TryGetValue("LocalRpcEndpoint", out var localTcpEndpoint))
            {
                return localTcpEndpoint;
            }

            var rpcOptions = EngineContext.Current.GetOptionsSnapshot<RpcOptions>();
            var host = GetHostIp(rpcOptions.Host);
            var port = rpcOptions.Port;
            var address = new SilkyEndpoint(host, port, ServiceProtocol.Rpc);
            _ = _rpcEndpointCache.TryAdd("LocalRpcEndpoint", address);
            return address;
        }

        public static bool IsLocalRpcAddress(string address)
        {
            var localAddress = GetLocalRpcEndpoint().GetAddress();
            return localAddress.Equals(address);
        }

        public static string GetLocalAddress()
        {
            var host = GetAnyHostIp();
            return host;
        }


        public static ISilkyEndpoint GetEndpoint(int port, ServiceProtocol serviceProtocol)
        {
            var host = GetIp(GetAnyHostIp());
            var address = new SilkyEndpoint(host, port, serviceProtocol);
            return address;
        }

        public static ISilkyEndpoint GetWsEndpoint()
        {
            if (_rpcEndpointCache.TryGetValue("WsEndpoint", out var wsEndpoint))
            {
                return wsEndpoint;
            }

            var webSocketOptions = EngineContext.Current.GetOptions<WebSocketOptions>();
            var host = GetHostIp(GetAnyHostIp());
            var address = new SilkyEndpoint(host, webSocketOptions.Port, ServiceProtocol.Ws);
            _rpcEndpointCache.TryAdd("WsEndpoint", wsEndpoint);
            return address;
        }

        public static int GetWsPort()
        {
            var webSocketOptions = EngineContext.Current.GetOptions<WebSocketOptions>();
            return webSocketOptions.Port;
        }

        public static ISilkyEndpoint CreateRpcEndpoint(string host, int port, ServiceProtocol serviceProtocol)
        {
            var rpcEndpoint = new SilkyEndpoint(host, port, serviceProtocol);
            return rpcEndpoint;
        }

        public static ISilkyEndpoint CreateRpcEndpoint(string address, ServiceProtocol serviceProtocol)
        {
            var addressInfo = address.Split(":");
            // var address = new RpcEndpoint(host, port, serviceProtocol);
            return CreateRpcEndpoint(addressInfo[0], addressInfo[1].To<int>(), serviceProtocol);
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