using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.Utils
{
    public static class ByteUtils
    {
        public static int IndexOf(byte[] array, byte[] pattern, int offset)
        {
            int success = 0;
            for (int i = offset; i < array.Length; i++)
            {
                if (array[i] == pattern[success])
                {
                    success++;
                }
                else
                {
                    success = 0;
                }

                if (pattern.Length == success)
                {
                    return i - pattern.Length + 1;
                }
            }
            return -1;
        }

        public static bool HasPrefix(byte[] array, byte[] pattern, int offset)
        {
            return IndexOf(array, pattern, offset) == 0;
        }

        //ReadOnlySpan<byte> - NetStandart2
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return a1.SequenceEqual(a2);
        }


        // reverse byte order (16-bit)
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }


        // reverse byte order (32-bit)
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

   
        // reverse byte order (64-bit)
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public static string ToHexString(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (var b in hex)
            {
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }


        public static int IndexOfPattern<T>(T[] array, T[] pattern, int startIndex, int count)
        {
            int fidx = 0;
            int result = Array.FindIndex<T>(array, startIndex, count, (T item) => {
                fidx = (item.Equals(pattern[fidx]) ? (fidx + 1) : 0);
                return (fidx == pattern.Length);
            });
            return (result < 0) ? -1 : result - fidx + 1;
        }

    }
}