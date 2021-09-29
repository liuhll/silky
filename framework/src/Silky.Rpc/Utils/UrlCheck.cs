using System;
using System.Net;

namespace Silky.Rpc.Utils
{
    public static class UrlCheck
    {
        public static bool UrlIsValid(string url, out string exMessage)
        {
            try
            {
                exMessage = string.Empty;
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 5000;
                request.Method = "HEAD";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                exMessage = ex.Message;
                return false;
            }
        }
    }
}