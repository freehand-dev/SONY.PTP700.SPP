using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SONY.PTP700.SPP.Utils
{
    public static class MemoryStreamExtensions
    {
        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }

        public static void Append(this MemoryStream stream, byte[] values, int count)
        {
            stream.Write(values, 0, count);
        }

    }
}
