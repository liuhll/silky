using Silky.Core.Extensions;
using System;
using System.Linq;

namespace Silky.Rpc.Utils
{
    public static class UrlUtil

    {
        public static bool IsHealth(string url, out string exMessage)
        {
            try
            {
                var urlInfo = Parser(url,string.Empty);
                exMessage = "";
                return SocketCheck.TestConnection(urlInfo.Item2, urlInfo.Item3);
            }
            catch (Exception ex)
            {
                exMessage = ex.Message;
                return false;
            }
        }

        public static (string, string, int) Parser(string url,string cidr)
        {
            var addressSegments = url.Split("://");
            var scheme = addressSegments.First();

            var address = addressSegments.Last();
            var domainAndPort = address.TrimEnd('/').Split(":");
            string host;
            if (address.Contains("+") || address.Contains("*") ||
                address.Contains("0.0.0.0") || address.Contains("[::]"))
            {
                
                host = HostAddressUtil.GetLocalHostAnyIp(cidr);
            }
            else
            {
                host = domainAndPort[0];
            }

            int port;
            if (domainAndPort.Length >= 2)
            {
                port = domainAndPort.Last().To<int>();
            }
            else
            {
                if ("https".Equals(scheme))
                {
                    port = 443;
                }
                else
                {
                    port = 80;
                }
            }

            return (scheme, host, port);
        }
    }
}