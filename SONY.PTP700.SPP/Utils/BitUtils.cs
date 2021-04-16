using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.Utils
{
    public static class BitUtils
    {


        static public bool IsBitSet(byte? value, int bitNumber)
        {
            if ((bitNumber < 0) || (bitNumber > 7))
            {
                throw new ArgumentOutOfRangeException("bitNumber", bitNumber, "bitNumber must be 0..7");
            }

            return ((value & (1 << bitNumber)) != 0);
        }


        static public void BitSet(ref byte? value, int bitNumber, bool level)
        {
            if ((bitNumber < 0) || (bitNumber > 7))
            {
                throw new ArgumentOutOfRangeException("bitNumber", bitNumber, "bitNumber must be 0..7");
            }

            value = (level) ? (byte)(value | (1 << bitNumber)) : (byte)(value & ~(1 << bitNumber));
        }

    }
}
