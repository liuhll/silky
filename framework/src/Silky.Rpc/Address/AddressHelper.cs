using System.Net;
using System.Text.RegularExpressions;

namespace Silky.Rpc.Address
{
    public class AddressHelper
    {
        public static string GetIp(string address)
        {
            if (IsValidIp(address))
            {
                return address;
            }
            var ips = Dns.GetHostAddresses(address);
            return ips[0].ToString();
        }

        public static bool IsValidIp(string address)
        {
            if (Regex.IsMatch(address, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                string[] ips = address.Split('.');
                if (ips.Length == 4 || ips.Length == 6)
                {
                    if (int.Parse(ips[0]) < 256 && int.Parse(ips[1]) < 256 && int.Parse(ips[2]) < 256 && int.Parse(ips[3]) < 256)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }
}