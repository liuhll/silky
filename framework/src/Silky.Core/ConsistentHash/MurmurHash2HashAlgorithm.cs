using System;
using System.Text;

namespace Silky.Core
{
    public class MurmurHash2HashAlgorithm : IHashAlgorithm
    {
        public int Hash(string item)
        {
            uint hash = Hash(Encoding.UTF8.GetBytes(item));
            return (int)hash;
        }

        private const UInt32 m = 0x5bd1e995;
        private const Int32 r = 24;

        public static UInt32 Hash(Byte[] data)
        {
            return Hash(data, 0xc58f1a7b);
        }

        public static UInt32 Hash(Byte[] data, UInt32 seed)
        {
            Int32 length = data.Length;
            if (length == 0)
                return 0;

            UInt32 h = seed ^ (UInt32)length;
            Int32 c = 0; // current index
            while (length >= 4)
            {
                UInt32 k = (UInt32)(
                    data[c++]
                    | data[c++] << 8
                    | data[c++] << 16
                    | data[c++] << 24);
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }

            switch (length)
            {
                case 3:
                    h ^= (UInt16)(data[c++] | data[c++] << 8);
                    h ^= (UInt32)(data[c] << 16);
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(data[c++] | data[c] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[c];
                    h *= m;
                    break;
                default:
                    break;
            }

            // Do a few final mixes of the hash to ensure the last few
            // bytes are well-incorporated.

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}