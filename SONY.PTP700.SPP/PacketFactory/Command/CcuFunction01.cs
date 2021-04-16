using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SONY.PTP700.SPP.Utils;

namespace SONY.PTP700.SPP.PacketFactory.Command
{
    public class CcuFunction01 : Message50.SPpCommandPair
    {
        public enum CmdGPEnum : byte
        {
            Relative = 0x40,
            Absolute = 0x41,
        }
        static public byte _PARAM_0 = 0x12;

        public CcuFunction01.CmdGPEnum CmdGP
        {
            get
            {
                return (CcuFunction01.CmdGPEnum)this.CMD_GP;
            }
            set
            {
                this.CMD_GP = (byte)value;
            }
        }

        public bool Mono
        {
            get
            {
                return Utils.BitUtils.IsBitSet(this.PARAM1, 2);
            }
            set
            {
                var param_1 = this.PARAM1;
                Utils.BitUtils.BitSet(ref param_1, 2, value);
                this.PARAM1 = param_1;
            }
        }

        public CcuFunction01(CcuFunction00.CmdGPEnum cmdGP)
            : base((byte)cmdGP, CcuFunction00._PARAM_0)
        {

        }

        public CcuFunction01(byte[] data, int offset = 0)
            : base(data, offset)
        {

        }

    }
}
