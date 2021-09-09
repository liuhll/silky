using System;
using System.Net.NetworkInformation;
using Silky.Core;
using Silky.Core.Extensions;
using Microsoft.Extensions.Options;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Utils
{
    public static class NetUtil
    {
        private const string ANYHOST = "0.0.0.0";
        private const int MIN_PORT = 0;
        private const int MAX_PORT = 65535;

        private const string LOCAL_IP_PATTERN = "127(\\.\\d{1,3}){3}$";
        private const string LOCAL_HOSTADRRESS = "localhost";
        private const string IP_PATTERN = "\\d{1,3}(\\.\\d{1,3}){3,5}$";

        public static string GetHostAddress(string hostAddress)
        {
            var result = hostAddress;
            if ((!IsValidAddress(hostAddress) && !IsLocalHost(hostAddress)) || IsAnyHost(hostAddress))
            {
                result = GetAnyHostAddress();
            }

            return result;
        }

        private static string GetAnyHostAddress()
        {
            string result = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties pix = adapter.GetIPProperties();
                    UnicastIPAddressInformationCollection ipCollection = pix.UnicastAddresses;
                    foreach (UnicastIPAddressInformation ipaddr in ipCollection)
                    {
                        if (ipaddr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            result = ipaddr.Address.ToString();
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static IAddressModel GetRpcAddressModel()
        {
            var rpcOptions = EngineContext.Current.GetOptionsSnapshot<RpcOptions>();
            string host = GetHostAddress(rpcOptions.Host);
            int port = rpcOptions.Port;
            var address = new AddressModel(host, port, ServiceProtocol.Tcp);
            return address;
        }
        
        public static string GetLocalAddress()
        {
            string host = GetAnyHostAddress();
            return host;
        }


        public static IAddressModel GetAddressModel(int port, ServiceProtocol serviceProtocol)
        {
            string host = GetHostAddress(GetAnyHostAddress());
            var address = new AddressModel(host, port, serviceProtocol);
            return address;
        }

        public static IAddressModel GetAddressModel(string host, int port, ServiceProtocol serviceProtocol)
        {
            var address = new AddressModel(host, port, serviceProtocol);
            return address;
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