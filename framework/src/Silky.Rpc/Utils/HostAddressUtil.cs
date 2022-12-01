using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Silky.Core.Extensions;

namespace Silky.Rpc.Utils;

internal static class HostAddressUtil
{
    public static string GetLocalHostAnyIp()
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
}