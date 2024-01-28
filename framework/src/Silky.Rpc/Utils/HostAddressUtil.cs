using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;

namespace Silky.Rpc.Utils;

internal static class HostAddressUtil
{
    private static string GetLocalHostAnyIp()
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

    public static string GetLocalHostAnyIp(string cidrNotation)
    {
        if (cidrNotation.IsNullOrEmpty())
        {
            return GetLocalHostAnyIp();
        }

        IPAddress result = null;
        if (!TryParseCIDR(cidrNotation, out var networkAddress, out var subnetMaskLength))
        {
            throw new SilkyException("CIDR notation format error");
        }

        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var adapter in adapters)
        {
            var adapterProperties = adapter.GetIPProperties();
            foreach (var ipInfo in adapterProperties.UnicastAddresses)
            {
                if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipAddress = ipInfo.Address;
                    // 获取要判断的 IP 地址的网络地址

                    if (IsInRange(ipAddress, networkAddress, subnetMaskLength))
                    {
                        result = ipAddress;
                        break;
                    }
                }
            }

            if (result != null)
            {
                break;
            }
        }

        if (result == null)
        {
            throw new SilkyException("The IP address of the specified network segment does not exist");
        }

        return result.ToString();
    }


    private static bool IsInRange(IPAddress ipAddress, IPAddress networkAddress, int subnetMaskLength)
    {
        int ipAddr = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
        int cidrAddr = BitConverter.ToInt32(networkAddress.GetAddressBytes(), 0);
        int cidrMask = IPAddress.HostToNetworkOrder(-1 << (32 - subnetMaskLength));

        return ((ipAddr & cidrMask) == (cidrAddr & cidrMask));
    }


    // 解析CIDR表示法，获取网络地址和子网掩码长度
    static bool TryParseCIDR(string cidrNotation, out IPAddress networkAddress, out int subnetMaskLength)
    {
        string[] parts = cidrNotation.Split('/');
        if (parts.Length != 2)
        {
            networkAddress = null;
            subnetMaskLength = 0;
            return false;
        }

        string ipAddressString = parts[0];
        subnetMaskLength = int.Parse(parts[1]);

        networkAddress = IPAddress.Parse(ipAddressString);
        return true;
    }
    //
    // private static bool IsInSubnet(this IPAddress address2,  IPAddress subnetMask)
    // {
    //     var network2 = address2.GetNetworkAddress(subnetMask);
    //     return subnetMask.Equals(network2);
    // }
    //
    //
    //
    // private static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
    // {
    //     var network1 = address.GetNetworkAddress(subnetMask);
    //     var network2 = address2.GetNetworkAddress(subnetMask);
    //
    //     return network1.Equals(network2);
    // }
    //
    // private static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
    // {
    //     byte[] ipAdressBytes = address.GetAddressBytes();
    //     byte[] subnetMaskBytes = subnetMask.GetAddressBytes();
    //
    //     if (ipAdressBytes.Length != subnetMaskBytes.Length)
    //         throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
    //
    //     byte[] broadcastAddress = new byte[ipAdressBytes.Length];
    //     for (int i = 0; i < broadcastAddress.Length; i++)
    //     {
    //         broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
    //     }
    //
    //     return new IPAddress(broadcastAddress);
    // }
}