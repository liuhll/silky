using System;
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
    public static class RpcEndpointHelper
    {
        private const string ANYHOST = "0.0.0.0";
        private const string LOCAL_IP_PATTERN = "127(\\.\\d{1,3}){3}$";
        private const string LOCAL_HOSTADRRESS = "localhost";
        private const string IP_PATTERN = "\\d{1,3}(\\.\\d{1,3}){3,5}$";

        public static string GetHostIp(string hostAddress)
        {
            var result = hostAddress;
            if ((!IsValidAddress(hostAddress) && !IsLocalHost(hostAddress)) || IsAnyHost(hostAddress))
            {
                result = GetAnyHostIp();
            }

            return result;
        }

        public static RpcEndpointDescriptor GetLocalWebEndpointDescriptor()
        {
            return GetLocalWebEndpoint()?.Descriptor;
        }

        public static IRpcEndpoint GetLocalWebEndpoint()
        {
            var server = EngineContext.Current.Resolve<IServer>();
            Check.NotNull(server, nameof(server));
            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
            if (address.IsNullOrEmpty())
            {
                //throw new SilkyException("Failed to obtain http service rpcEndpoint");
                return null;
            }

            var addressDescriptor = ParseRpcEndpointDescriptor(address);
            return addressDescriptor;
        }

        private static IRpcEndpoint ParseRpcEndpointDescriptor(string address)
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
            return new RpcEndpoint(host, port, serviceProtocol);
        }

        private static string GetAnyHostIp()
        {
         
            UnicastIPAddressInformation mostSuitableIp = null;
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;
                var properties = network.GetIPProperties();
                if (properties.GatewayAddresses.Count == 0)
                    continue;
                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }

                    return address.Address.ToString();
                }
                
            }

            return mostSuitableIp != null 
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        public static IRpcEndpoint GetLocalTcpEndpoint()
        {
            var rpcOptions = EngineContext.Current.GetOptionsSnapshot<RpcOptions>();
            string host = GetHostIp(rpcOptions.Host);
            int port = rpcOptions.Port;
            var address = new RpcEndpoint(host, port, ServiceProtocol.Tcp);
            return address;
        }

        public static bool IsLocalRpcAddress(string address)
        {
            var localAddress = RpcEndpointHelper.GetLocalTcpEndpoint().GetAddress();
            return localAddress.Equals(address);
        }

        public static string GetLocalAddress()
        {
            string host = GetAnyHostIp();
            return host;
        }


        public static IRpcEndpoint GetRpcEndpoint(int port, ServiceProtocol serviceProtocol)
        {
            string host = GetIp(GetAnyHostIp());
            var address = new RpcEndpoint(host, port, serviceProtocol);
            return address;
        }

        public static IRpcEndpoint GetWsEndpoint()
        {
            var webSocketOptions = EngineContext.Current.GetOptions<WebSocketOptions>();
            string host = GetHostIp(GetAnyHostIp());
            var address = new RpcEndpoint(host, webSocketOptions.Port, ServiceProtocol.Ws);
            return address;
        }

        public static int GetWsPort()
        {
            var webSocketOptions = EngineContext.Current.GetOptions<WebSocketOptions>();
            return webSocketOptions.Port;
        }

        public static IRpcEndpoint CreateRpcEndpoint(string host, int port, ServiceProtocol serviceProtocol)
        {
            var rpcEndpoint = new RpcEndpoint(host, port, serviceProtocol);
            return rpcEndpoint;
        }

        public static IRpcEndpoint CreateRpcEndpoint(string address, ServiceProtocol serviceProtocol)
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