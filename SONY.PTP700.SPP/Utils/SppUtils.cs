using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.Utils
{
    public static class SppUtils
    {
        public static ushort RcpIdToRaw(byte rcpID, bool enabled)
        {
            ushort value = (enabled) ? (ushort)(0x0001 & 0xffff) : (ushort)(0x0000 & 0xffff);
            value += (ushort)(((byte)rcpID << 4) & 0xffff);
            return value;
        }


        public static (byte id, bool enabled) ParseRcpID(ushort id)
        {
            bool _enabled = (id >> 13) == 1;
            byte _id = (byte)(((id >> 1) << 1) >> 4);
            return (_id, _enabled);
        }



    }
}
