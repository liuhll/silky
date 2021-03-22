using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Address;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Utils
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
            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
            string host = GetHostAddress(rpcOptions.Host);
            int port = rpcOptions.RpcPort;
            var address = new AddressModel(host, port, ServiceProtocol.Tcp);
            return address;
        }

        public static IAddressModel GetAddressModel(int port, ServiceProtocol serviceProtocol)
        {
            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
            string host = GetHostAddress(GetAnyHostAddress());
            var address = new AddressModel(host, port, serviceProtocol);
            return address;
        }

        public static IAddressModel GetAddressModel(string host, int port, ServiceProtocol serviceProtocol)
        {
            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
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