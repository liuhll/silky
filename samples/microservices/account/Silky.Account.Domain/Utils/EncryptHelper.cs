using System;
using System.Security.Cryptography;
using System.Text;

namespace Silky.Account.Domain.Utils
{
    public static class EncryptHelper
    {
        public static string Md5(string line, int bit)
        {
            var md5 = MD5.Create(); 
            var hashedDataBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(line));
            var tmp = new StringBuilder();
            foreach (var i in hashedDataBytes) tmp.Append(i.ToString("x2"));
            if (bit == 16)
                return tmp.ToString().Substring(8, 16);
            if (bit == 32)

                return tmp.ToString();
            return string.Empty;
        }

        public static string Md5(string line, MD5Length length = MD5Length.L32)
        {
            var str_md5_out = string.Empty;
            using (var md5 = MD5.Create())
            {
                var bytes_md5_in = Encoding.UTF8.GetBytes(line);
                var bytes_md5_out = md5.ComputeHash(bytes_md5_in);

                str_md5_out = length == MD5Length.L32
                    ? BitConverter.ToString(bytes_md5_out)
                    : BitConverter.ToString(bytes_md5_out, 4, 8);

                str_md5_out = str_md5_out.Replace("-", "");
                return str_md5_out.ToLower();
            }
        }
    }

    public enum MD5Length
    {
        L16,
        L32
    }
}