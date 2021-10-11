using System.Text;
using JetBrains.Annotations;

namespace Silky.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string GetString(this byte[] bytes)
        {
            return bytes.GetString(Encoding.UTF8);
        }

        public static string GetString([NotNull] this byte[] bytes, [NotNull] Encoding encoding)
        {
            Check.NotNull(bytes, nameof(bytes));
            Check.NotNull(encoding, nameof(encoding));
            return encoding.GetString(bytes);
        }

        public static bool Equals([NotNull] this byte[] data1, [NotNull] byte[] data2)
        {
            Check.NotNull(data1, nameof(data1));
            Check.NotNull(data2, nameof(data2));
            if (data1.Length != data2.Length)
                return false;
            for (var i = 0; i < data1.Length; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }

            return true;
        }
    }
}