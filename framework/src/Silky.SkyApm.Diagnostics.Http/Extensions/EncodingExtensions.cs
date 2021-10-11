using System;
using System.Text;

namespace Silky.SkyApm.Diagnostics.Rpc.Http.Extensions
{
    internal static class EncodingExtensions
    {
        public static Encoding ToEncoding(this string charset, Encoding fallbackDefault)
        {
            try
            {
                if (string.IsNullOrEmpty(charset))
                    return fallbackDefault;

                return Encoding.GetEncoding(charset);
            }
            catch (Exception)
            {
                return fallbackDefault;
            }
        }
    }
}