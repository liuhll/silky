using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private static ConcurrentDictionary<ServiceProtocol, IAddressModel> addressModels =
            new ConcurrentDictionary<ServiceProtocol, IAddressModel>();


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

        public static IAddressModel GetHostAddress(ServiceProtocol serviceProtocol)
        {
            if (addressModels.TryGetValue(serviceProtocol, out IAddressModel address))
            {
                return address;
            }

            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
            string host = GetHostAddress(rpcOptions.Host);
            int port;
            switch (serviceProtocol)
            {
                case ServiceProtocol.Tcp:
                    port = rpcOptions.RpcPort;
                    break;
                case ServiceProtocol.Mqtt:
                    port = rpcOptions.MqttPort;
                    break;
                default:
                    throw new LmsException("必须指定地址类型");
            }

            address = new AddressModel(host, port, serviceProtocol);
            return addressModels.GetOrAdd(serviceProtocol, address);
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