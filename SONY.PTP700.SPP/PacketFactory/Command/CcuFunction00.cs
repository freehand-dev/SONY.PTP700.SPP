using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SONY.PTP700.SPP.Utils;

namespace SONY.PTP700.SPP.PacketFactory.Command
{
    public class CcuFunction00 : Message50.SPpCommandPair
    {
        public enum CmdGPEnum : byte
        {
            Relative = 0x40,
            Absolute = 0x41,
        }
        static public byte _PARAM_0 = 0x10;

        public CcuFunction00.CmdGPEnum CmdGP
        {
            get
            {
                return (CcuFunction00.CmdGPEnum)this.CMD_GP;
            }
            set
            {
                this.CMD_GP = (byte)value;
            }
        }

        // ::BUTTON::BARS
        public bool Bars
        {
            get
            {
                return Utils.BitUtils.IsBitSet(this.PARAM1, 0);
            }
            set
            {
                var param_1 = this.PARAM1;
                Utils.BitUtils.BitSet(ref param_1, 0, value);
                this.PARAM1 = param_1;
            }
        }

        // ::MENU::CONFIG::CCU::MODE::CHROMA'
        public bool Chroma
        {
            get
            {
                return Utils.BitUtils.IsBitSet(this.PARAM1, 1);
            }
            set
            {
                var param_1 = this.PARAM1;
                Utils.BitUtils.BitSet(ref param_1, 1, value);
                this.PARAM1 = param_1;
            }
        }

        public bool CCUSkinGate
        {
            get
            {
                return Utils.BitUtils.IsBitSet(this.PARAM1, 6);
            }
            set
            {
                var param_1 = this.PARAM1;
                Utils.BitUtils.BitSet(ref param_1, 6, value);
                this.PARAM1 = param_1;
            }
        }


        public CcuFunction00(CcuFunction00.CmdGPEnum cmdGP)
            : base((byte)cmdGP, CcuFunction00._PARAM_0)
        {

        }

        public CcuFunction00(byte[] data, int offset = 0)
            : base(data, offset)
        {

        }

    }
}
